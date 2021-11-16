using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBox.MVVModels;

namespace MusicBox.Models
{
    public class LastFramesList
    {
        private readonly List<List<int>> _numberFrames;
        private readonly int _capacity;
        public LastFramesList(int framesCount)
        {
            _numberFrames = new List<List<int>>(framesCount);
            _capacity = framesCount;
        }
        public void Add(List<ComingItem> frame)
        {
            if (_numberFrames.Count >= _capacity)
            {
                _numberFrames.RemoveAt(0);
            }
            var tempList = new List<int>();
            foreach (var item in frame)
            {
                tempList.Add(item.Number);
            }
            _numberFrames.Add(tempList);
        }

        public bool ContainNumber(int number)
        {
            foreach (var frame in _numberFrames)
            {
                foreach (var item in frame)
                {
                    if (item == number)
                        return true;
                }
            }
            return false;
        }
    }
}
