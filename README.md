# MMXD Source

This repo contains the original source code for the game Mega Man X DiVE Offline 1.0.0 version from Steam.

Unity Version: 2020.3.48f1 (the last version from 2020)

## Instructions 

Just clone this repo (or manually download it) and open in the Unity 2020.3.48f1. 

The project is big so the entire process can take some time.


## Custom Content

Some custom code and shaders were added in order to improve the project.

### Visualizing the Prefab Models

If you try to inspect the project Prefabs, you will find them without textures applied. 

![MMX without textures](/Docs/MMX_WithoutTextures.png "MMX without textures")

This happens because the game uses a process where the textures and other materials properties are only set in runtime (when the game runs).

To fix this and visualize the Prefabs in the Editor:

1. Select a Prefab with the model you want to visualize;
2. Right click on any `Character Material` component and select the last option `Update Materials`.

![Update Materials](/Docs/UpdateMaterials.png "Update Materials")

A Materials folder with the correct materials will be created at the same location your Prefab is.

![MMX with textures](/Docs/MMX_WithTextures.png "MMX with textures")

Some Prefabs like CH001_000_G_0, CH001_000_U_0, CH001_000_U_S (Mega Man X normal armor) are already proper set using this tool.

> **Note**: This tool **only works with Prefabs**, no matter if you are inspecting the Prefab on Scene Hierarchy, Prefab Mode or Project window.

## How To

### Find Sprite Source

Many sprites are in the `.asset` format. This is a Unity format and cannot be used in other softwares (like Photoshop).

![Asset Sprite](/Docs/AssetSprite.png "Asset Sprite")

To use them, we need to find the texture source.

Open the file using a text editor and look for the first `texture:` key. Copy the guid number:

![Asset Sprite Texture Guid](/Docs/AssetSpriteTextureGuid.png "Asset Sprite Texture Guid")

On Unity, open the Quick Search Window using **Help > Quick Search** (`Alt + '`) and paste the guid number there. The texture source will be there:

![Sprite Texture](/Docs/SpriteTexture.png "Sprite Texture")

Sometime the Sprite will be in a Sprite Atlas Texture, with other ones. If you just need a single sprite from this atlas, you should extract it.

First, create a Sprite inside this Texture Atlas, using the same name, pivot and border from the original one.

![MMX_Icon](/Docs/MMX_Icon.png "MMX_Icon")
![MMX_Sprite](/Docs/MMX_Sprite.png "MMX_Sprite")

After that, click in the new created sprite, go to the Context Menu and Extract this sprite as a new file:

![MMX_Sprite_Extract](/Docs/MMX_Sprite_Extract.png "MMX_Sprite_Extract")