using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghbvft6.CopypasteMapper {
    public class Settings {
        public class Mapping {
            public string matchPattern = "";
            public string replacementPattern = "";
            public string replacement = "";
        }

        public int delay = 100;
        public Mapping[] mappings = Array.Empty<Mapping>();
    }
}
