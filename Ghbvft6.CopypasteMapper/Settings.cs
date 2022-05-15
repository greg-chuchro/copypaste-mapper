using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghbvft6.CopypasteMapper {
    public class Settings {
        public class Mapping {
            public string MatchPattern;
            public string ReplacementPattern;
            public string Replacement;
        }

        public int delay;
        public Mapping[] mappings;
    }
}
