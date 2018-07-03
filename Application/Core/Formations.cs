using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using static Godot.GD;
using File = Godot.File;
using Newtonsoft.Json;

namespace SpaceShooter.Application.Core
{
    public class Formations
    {
        // formations are groups of spawn positions. Each spawn has a 1 or 0 where 1 means to spawn said enemy and 0
        // is to ignore. Gives us the opportunity to make cool formations if we wanted to.
        private List<List<List<int>>> _formationList = new List<List<List<int>>>();

        public int CurrentColumn = 0;
        public int CurrentFormation = 0;

        public int XPosition = 1364;
        
        // y positions for each enemy for each column
        public int[] Positions =
        {
            160, 320, 480, 640
        };

        private string _formationDataPath = "res://Data/formations.json";

        // todo: make a note talking about converting formation list into json data
        public Formations()
        {
            LoadJson();
            SelectRandomFormation();
        }

        public int[] Spawns()
        {
            var spawnCount = 0;
            int[] spawns = {
                0, 0, 0, 0
            };

            var formation = _formationList[CurrentFormation];
            if(formation != null)
            {
                foreach(var spawn in formation)
                {
                    spawns[spawnCount] = spawn[CurrentColumn];
                    ++spawnCount;
                }
            }

            return spawns;
        }

        public void SelectRandomFormation()
        {
            CurrentFormation = GameScene.Rand.Next(0, _formationList.Count);
        }

        public void NextColumn()
        {
            if(_formationList.Count == 0) return;

            // we select a random column if we hit the end of the line or we append +1 and move to the
            // next column
            if(IsEndOfColumn(out var nextColumn))
            {
                CurrentColumn = 0;
                SelectRandomFormation();
            }
            else
            {
                CurrentColumn = nextColumn;
            }
        }

        public bool IsEndOfColumn(out int outColumn)
        {
            outColumn = CurrentColumn + 1;
            var currentFormation = _formationList[CurrentFormation];

            return outColumn == currentFormation.Count;
        }

        private void LoadJson()
        {
            var file = new File();
            if(file.Open(_formationDataPath, (int) File.ModeFlags.Read) != Error.Ok)
            {
                file.Close();
                throw new FileLoadException($"There was a problem loading: {_formationDataPath}");
            }

            var data = file.GetAsText();
            file.Close();

            _formationList = JsonConvert.DeserializeObject<List<List<List<int>>>>(data);
        }
        
    }
}