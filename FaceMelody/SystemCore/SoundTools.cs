using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace FaceMelody.SystemCore
{
    class SoundTools
    {

        public List<int> sound_reader(string file)
        {
            List<int> ret = new List<int>();
            SoundPlayer sound_player = new SoundPlayer(file);
            //sound_player.Stream
            return ret;
        }
    }
}
