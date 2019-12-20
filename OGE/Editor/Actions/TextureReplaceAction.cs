using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using OGE.Editor.Interfaces;
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

        public void Init(CacheFile target, PegEntry selectedEntry, string replacementFilePath)
        {
            PegName = target.PegData.cpuFileName;
            SubTextureName = selectedEntry.Name;
            //Todo: Set TrackedByAsm and AsmName

            selectedEntry.Bitmap = new Bitmap(replacementFilePath);
            selectedEntry.Edited = true;
            selectedEntry.RawData = Utility.Helpers.ImageHelpers.ConvertBitmapToByteArray(selectedEntry.Bitmap);

            uint original_width = selectedEntry.width;

            selectedEntry.width = (ushort)selectedEntry.Bitmap.Width;
            selectedEntry.height = (ushort)selectedEntry.Bitmap.Height;
            selectedEntry.source_height = (ushort)selectedEntry.Bitmap.Height;

            //source_width sometimes equals width and sometimes equals 36352. This is a quick hack for now until that behavior is understood.
            //Not properly setting this causes the game to improperly scale the texture
            if (selectedEntry.source_width == original_width)
            {
                selectedEntry.source_width = selectedEntry.width;
            }
            else
            {
                selectedEntry.source_width = 36352;
            }

            UpdateDescription();
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
