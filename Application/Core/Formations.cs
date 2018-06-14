using System.Collections.Generic;

namespace SpaceShooter.Application.Core
{
    public class Formations
    {
        // formations are groups of spawn positions. Each spawn has a 1 or 0 where 1 means to spawn said enemy and 0
        // is to ignore. Gives us the opportunity to make cool formations if we wanted to. We could've also done this
        // using json.
        public readonly List<Formation> FormationList = new List<Formation>();

        public int CurrentColumn = 0;
        public int CurrentFormation = 0;

        public int XPosition = 1364;
        
        // y positions for each enemy for each column
        public int[] Positions =
        {
            160, 320, 480, 640
        };

        // todo: make a note talking about converting formation list into json data
        public Formations()
        {
            // formation 1
            FormationList.Add(new Formation
            {
                Columns = new List<Spawn>
                {
                    new Spawn(1, 0, 1, 0),
                    new Spawn(0, 1, 0, 1),
                    new Spawn(1, 0, 1, 0),
                    new Spawn(0, 1, 0, 1)
                }
            });
            
            // formation 2
            FormationList.Add(new Formation
            {
                Columns = new List<Spawn>
                {
                    new Spawn(1, 0, 1, 0),
                    new Spawn(),
                    new Spawn(),
                    new Spawn(1, 0, 1, 0)
                }
            });
            
            // formation 3
            FormationList.Add(new Formation
            {
                Columns = new List<Spawn>
                {
                    new Spawn(1, 0, 0, 1),
                    new Spawn(0, 1, 1, 0),
                    new Spawn(0, 1, 1, 0),
                    new Spawn(1, 0, 0, 1)
                }
            });
            
            SelectRandomFormation();
        }

        public int[] Spawns()
        {
            var spawnCount = 0;
            int[] spawns = {
                0, 0, 0, 0
            };
            
            var formation = FormationList[CurrentFormation];
            if(formation != null)
            {
                foreach(var spawn in formation.Columns)
                {
                    var currentSpawn = spawn.Points[CurrentColumn];
                    spawns[spawnCount] = currentSpawn;
                    ++spawnCount;
                }
            }

            return spawns;
        }

        public void SelectRandomFormation()
        {
            CurrentFormation = GameScene.Rand.Next(0, FormationList.Count);
        }

        public void NextColumn()
        {
            if(FormationList.Count == 0) return;

//            var nextColumn = CurrentColumn + 1;
//            var currentFormation = FormationList[CurrentFormation];
            int nextColumn;
            
            // we select a random column if we hit the end of the line or we append +1 and move to the
            // next column
            if(IsEndOfColumn(out nextColumn))
            {
                CurrentColumn = 0;
                SelectRandomFormation();
            }
            else
            {
                CurrentColumn = nextColumn;
            }
        }

        public bool IsEndOfColumn()
        {
            int nextColumn;

            return IsEndOfColumn(out nextColumn);
        }

        public bool IsEndOfColumn(out int outColumn)
        {
            outColumn = CurrentColumn + 1;
            var currentFormation = FormationList[CurrentFormation];

            return outColumn == currentFormation.Columns.Count;
        }
        
    }
}