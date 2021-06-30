using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Social;
using static BoardGames.Misc.SoundSetEvent;

namespace BoardGames.Misc {
    public class SoundSet {
        public static SoundSet Dice {
            get {
                List<(SoundSetEvent, float[])> events = new List<(SoundSetEvent, float[])> { };
                float force = Main.rand.NextFloat(16, 24);
                events.Add((PLAY, new float[] {21, 0}));
                events.Add((DATA, new float[] {0.5f, force/24f}));
                while(force>2) {
                    events.Add((WAIT, new float[] {force/2}));
                    events.Add((PLAY, new float[] {21, 0}));
                    events.Add((DATA, new float[] {0.5f, force/24f}));
                    force *= Main.rand.NextFloat(0.35f, 1);
                }
                return new SoundSet(events.ToArray());
            }
        }
        (SoundSetEvent _event, float[] data)[] events;
        int delay = 0;
        int index = 0;
        SoundEffectInstance latest;
        public SoundSet(params (SoundSetEvent, float[])[] events) {
            this.events = events;
        }
        public bool Update() {
            if(index<events.Length) {
                if(delay>0) {
                    delay--;
                } else {
                    SoundEffectInstance sound = null;
                    effect:
                    var curr = events[index];
                    switch(curr._event) {
                        case PLAY:
                        sound = Main.PlaySound(new LegacySoundStyle((int)curr.data[0], (int)curr.data[1]));
                        break;
                        case DATA:
                        sound.Pitch = curr.data[0];
                        if(curr.data.Length>1)
                            sound.Volume*=curr.data[1];
                        break;
                        case WAIT:
                        delay = (int)curr.data[0];
                        break;
                    }
                    index++;
                    if(index<events.Length&&events[index]._event == DATA) {
                        goto effect;
                    }
                }
            } else {
                return true;
            }
            return false;
        }
    }
    public enum SoundSetEvent {
        PLAY,
        DATA,
        WAIT
    }
}
