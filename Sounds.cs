namespace Raycaster;
using SFML;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using Raycaster;
using System.Diagnostics;

public class Sounds
{
    public Sounds()
    {
        this.shotgunSound = new SoundBuffer("C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\sound\\shotgun.wav");
    }

    public SoundBuffer shotgunSound { get; set; }
    
    public Music themeMusic { get; set; } 

    public void playSound(SoundBuffer soundBuff)
    {
       Sound sound = new Sound(soundBuff);
       sound.Play();
    }
}