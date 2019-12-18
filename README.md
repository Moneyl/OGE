# OGE
Open Guerrilla Editor. An open source modding tool for Red Faction Guerrilla. Still in it's very early stages. The current goal is to make a comprehensive modding tool for RFG that automates some things that people used to do by hand such as updating asm_pc files, and eliminates the need for multiple command line tools. As it's expanded some existing tools like the [new texture editor](https://github.com/Moneyl/RFG-Texture-Editor-Redux) will likely be merged with this tool since there are some convenience features that can be provided by having both tools in one program.

Below is an image of the editor in it's early state. Note that the two tabs on the left side labeled "map view" and "modinfo generator" do nothing currently and are remnants from visual prototyping.

![alt text](https://github.com/Moneyl/OGE/blob/master/Repo/Example1.jpg "File explorer on the left, and texture viewer in the center.")
On the left is the file explorer for the working directory, in the center is the texture viewer, with a tab visible for a lua ui file.

## Features
This section lists current and planned features, and will be updated as new features are added.
- [x] Packfile introspection, and automatic extraction. Lets you view the contents of vpp_pc and str2_pc files without needing to fully extract them (some exceptions). Also automates extracting and caching files. No longer need to manually extract files.
- [x] Simple text editor for viewing xml, lua, and txt files for convenience. This is a placeholder which will be replaced with better tools such as a new [table file editor](https://github.com/volition-inc/Kinzies-Toy-Box/tree/master/tools/table_file_editor) later on.
- [x] Texture file viewing, and extraction.
- [ ] Texture editing and replacement. Includes automatic asm_pc edits so the game knows the new sizes of the edited textures.
- [ ] Modinfo.xml generation. OGE should track all file changes made, and use those to generate a modinfo.xml file for use with the mod manager.
- [ ] Zone/map/mission editing. Transfer the [zone editor](https://github.com/Moneyl/RFGR-Zone-Tools) to OGE and fix issues with zone editing breaking the map.
- [ ] New table file editor. See the [old one](https://github.com/volition-inc/Kinzies-Toy-Box/tree/master/tools/table_file_editor). Will attempt to provide descriptions of xtbl values, detect editing errors, and suggest element values.
- [ ] Scriptx visual editor. Scriptx is an xml scripting language used by RFG for missions and activies. It has [many ways](https://www.factionfiles.com/ff.php?action=scriptxfuncs) to interact with the game, but is difficult to edit. It should be possible to make something similar to [Unreal Engine blueprints](https://docs.unrealengine.com/en-US/Engine/Blueprints/index.html), but simpler, which generates scriptx files for the game to use.
- [ ] Tools for many of the games other custom formats. At current count there are ~26 RFG file formats that have no tool for editing or viewing them.
