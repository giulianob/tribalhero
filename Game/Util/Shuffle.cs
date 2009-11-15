using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Setup;

namespace Game.Util {
    public class Shuffle<T> {
        public static void shuffleArrayList(ArrayList al) {
            for (int i = 0; i < al.Count; i++) {
                object x = al[i];
                int index = Config.Random.Next(al.Count - i) + i;
                al[i] = al[index];
                al[index] = x;
            }
        }

        public static List<T> shuffleList(List<T> listToShuffle) {
            for (int k = listToShuffle.Count - 1; k > 1; --k) {
                int randIndx = Config.Random.Next(k); //
                T temp = listToShuffle[k];
                listToShuffle[k] = listToShuffle[randIndx]; // move random num to end of list.
                listToShuffle[randIndx] = temp;
            }
            return listToShuffle;
        }
    }
}
