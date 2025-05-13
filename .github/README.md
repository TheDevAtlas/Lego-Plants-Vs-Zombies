<h1 align="center">
  <a>
    <picture>
      <source height="125" media="(prefers-color-scheme: dark)" srcset="readmetitle.png">
      <img height="125" alt="LEGO Plants vs Zombies" src="readmetitle.png">
    </picture>
  </a>
</h1>

<div align="center">
  <table>
    <tr>
      <td align="center"><img src="Gameplay.gif" alt="Gameplay Loop" width="750"/></td>
    </tr>
  </table>
</div>

## 🌊 Abstract

The purpose of the project was to experiment with making my own LEGO game using official models from Mecabricks.com. By using community and self-built designs I was able to build small recreation of Plants vs Zombies.

It recreates core mechanics like seed selection, zombie waves, and tile-based combat. 16 plants are fully animated and useable as well as a decent codebase for adding new plants and abilities with most of the Plant and Zombie code being class based.

While this project is NOT a full game, there is a windows build. There is no win / lose state. If you would like to read more about how I made this project I have an article on the technical side of things as well as a YouTube video showing my process.

- **[Technical Article](https://thedevatlas.com/projects/lego-pvz/)**
- **[YouTube Video](https://youtu.be/F9z24XEOPYU?si=AqNc495CYTU1McKZ)**

## ⚙️ Installation & Setup

This project was made using **Unity 6000 LTS** with the Universal Render Pipeline package installed. However any modern version of Unity and URP should work. Follow these steps to set up the project:

1. **Clone the repository**:
   ```bash
   https://github.com/TheDevAtlas/Lego-Plants-Vs-Zombies
   ```

2. **Open with Unity Hub**:
   - Launch Unity Hub
   - Click "Add" and navigate to the cloned repository folder
   - Select the project folder and open it with the appropriate Unity version

3. **Verify Required Assets**:
   - Universal Render Pipeline package
   - Unity Recorder package
   - Text Mesh Pro

## 🌱 Project Details

The current project includes:

- 16 Animated LEGO Plants
- 5 Zombie Types
- Dynamic Level Sizes
- Custom LEGO Culling for Improved Performance
- Full Gameplay Loop from Beginning to End:
- Select Plants
- Collect Sun
- Plant
- Fight

Future work with LEGO includes better rendering techniques and improving how I organize and write code. This was mostly an exercise in making a Plants vs Zombies clone; however, making a LEGO game is a technical challenge in itself, requiring creative shortcuts and optimizations for rendering.

## 📚 References and Reading

If you’re interested in learning more about LEGO in games and the community of graphic designers who inspired this project, explore the links below:

LEGO:
- **[Rendering Realistic LEGO® Bricks](https://youtu.be/Xh_b9WDfZ-s?si=zSSJV9pk1TiACu9A)** - A video with the developers of LEGO Builder’s Journey detailing how they used Unity’s HDRP to create realistic LEGO bricks. It also covers their journey from URP to HDRP to ray tracing.

COMMUNITY / DESIGNS:
- **[Daniel Krafft](https://youtu.be/Df0cZH2hWd8?si=wRHx7jmMKDiUTD84)** - Graphic designer turns games into LEGO sets
- **[LEGO Ideas](https://ideas.lego.com/)** - Official site where LEGO has fans submit ideas