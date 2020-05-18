using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using OGE.Editor.Interfaces;
using OGE.Editor.Managers;
using RfgTools.Formats.Asm;
using RfgTools.Formats.Textures;
using RfgTools.Helpers;

namespace OGE.Editor.Actions
{
    /// <summary>
    /// Action used to track texture replacements.
    /// This involves replacing a sub-texture of a peg/vbm file
    /// with another texture.
    /// </summary>
    public class TextureReplaceAction : ITrackedAction
    {
        public string PegName { get; private set; }
        public string SubTextureName { get; private set; }
        public bool TrackedByAsm { get; private set; } = false;
        public string? AsmName { get; private set; } = null;
        string ITrackedAction.Description { get; set; } = "Not set";

        public TextureReplaceAction()
        {

        }

        public bool Init(CacheFile target, int selectedIndex, string replacementFilePath)
        {
            //Todo: Make some reuseable helper functions to shorten this up. Ex:
            //Todo:  - Copy file, parent, asm, and cpu/gpu sister file between caches
            //Todo:  - Repack parent with edited file
            //Todo:  - Update asm file with new values

            PegEntry selectedEntry = target.PegData.Entries[selectedIndex];
            PegName = target.PegData.cpuFileName;
            SubTextureName = selectedEntry.Name;

            //Set TrackedByAsm and find asm name
            CacheFile asmFile = null;
            AsmContainer targetContainer = null;
            AsmPrimitive targetPrimitive = null;
            if (target.Parent.PackfileData.ContainsAsmFiles)
            {
                TrackedByAsm = true;
                foreach (var asm in target.Parent.PackfileData.AsmFiles)
                {
                    foreach(var container in asm.Containers)
                    {
                        foreach (var primitive in container.Primitives)
                        {
                            if (primitive.Name != target.Filename) 
                                continue;

                            AsmName = primitive.Name;
                            targetContainer = container;
                            targetPrimitive = primitive;
                        }
                    }
                }

                if (AsmName == null || targetContainer == null || targetPrimitive == null)
                    return false;
            }

            //Copy edited files to project cache
            if (!ProjectManager.CopyFileToProjectCache(target.PegData.cpuFileName, target.Parent, out CacheFile cpuFile))
                return false;
            if (!ProjectManager.CopyFileToProjectCache(target.PegData.gpuFileName, target.Parent, out CacheFile gpuFile))
                return false;
            if (TrackedByAsm && !ProjectManager.CopyFileToProjectCache(AsmName, target.Parent, out asmFile) || asmFile == null)
                return false;

            //Update entry data
            selectedEntry = cpuFile.PegData.Entries[selectedIndex];
            selectedEntry.Bitmap = new Bitmap(replacementFilePath);
            selectedEntry.Edited = true;
            selectedEntry.RawData = Utility.Helpers.ImageHelpers.ConvertBitmapToByteArray(selectedEntry.Bitmap);
            selectedEntry.width = (ushort)selectedEntry.Bitmap.Width;
            selectedEntry.height = (ushort)selectedEntry.Bitmap.Height;
            selectedEntry.source_height = (ushort)selectedEntry.Bitmap.Height;

            //source_width sometimes equals width and sometimes equals 36352. This is a quick hack for now until that behavior is understood.
            //Not properly setting this causes the game to improperly scale the texture
            selectedEntry.source_width = selectedEntry.source_width == selectedEntry.width
                ? (ushort)selectedEntry.Bitmap.Width
                : (ushort)36352;
            cpuFile.PegData.Write(cpuFile.FilePath, gpuFile.FilePath);

            //Update asm data if applicable
            if (TrackedByAsm)
            {
                var cpuFileInfo = new FileInfo(cpuFile.FilePath);
                var gpuFileInfo = new FileInfo(gpuFile.FilePath);

                int sizeDifference = (int)cpuFileInfo.Length - (int)targetPrimitive.HeaderSize;
                sizeDifference += (int)gpuFileInfo.Length - (int)targetPrimitive.DataSize;

                targetPrimitive.HeaderSize = (uint)cpuFileInfo.Length;
                targetPrimitive.DataSize = (uint)gpuFileInfo.Length;

                //Todo: Finish packfile writing code
                //Todo: Pack str2 and update CompressedSize
                var parentPackfile = cpuFile.Parent.PackfileData;
                parentPackfile.WriteToBinary(Path.GetDirectoryName(asmFile.FilePath),
                    asmFile.Parent.FilePath, true, true, true);

                if (!cpuFile.Parent.PackfileData.TryGetSubfileEntry(cpuFile.Filename, out var cpuFileEntry))
                    return false;
                if (!cpuFile.Parent.PackfileData.TryGetSubfileEntry(gpuFile.Filename, out var gpuFileEntry))
                    return false;

                targetContainer.CompressedSize = parentPackfile.Header.CompressedDataSize;
                targetContainer.DataOffset = parentPackfile.DataStartOffset;
                targetPrimitive.HeaderSize = cpuFileEntry.DataSize;
                targetPrimitive.DataSize = gpuFileEntry.DataSize;

                //Todo: Update primitive sizes
                //Todo: Update DataOffset
                //Todo: Make sure to update other primitives in the same str2 if their offsets/values have changed

                asmFile.AsmData.WriteToBinary();
            }

            //Todo: Generate modinfo.xml data for this

            UpdateDescription();
            return true;
        }

        private void UpdateDescription()
        {
            ((ITrackedAction)this).Description = $"Replaced {SubTextureName} in {PegName} with an external texture.";
        }

        void ITrackedAction.Undo()
        {
            throw new NotImplementedException();
        }

        void ITrackedAction.Redo()
        {
            throw new NotImplementedException();
        }

        void ITrackedAction.WriteToProjectFile(XElement changeNode)
        {
            changeNode.Add(new XElement("PegName", PegName));
            changeNode.Add(new XElement("SubTextureName", SubTextureName));
            changeNode.Add(new XElement("TrackedByAsm", TrackedByAsm));
            if(AsmName != null)
                changeNode.Add(new XElement("AsmName", AsmName));
        }

        void ITrackedAction.ReadFromProjectFile(XElement changeNode)
        {
            PegName = changeNode.GetRequiredAttributeValue("PegName");
            SubTextureName = changeNode.GetRequiredAttributeValue("SubTextureName");
            TrackedByAsm = changeNode.GetRequiredAttributeValue("TrackedByAsm").ToBool();
            AsmName = changeNode.GetOptionalAttributeValue("AsmName", null);
        }
    }
}
