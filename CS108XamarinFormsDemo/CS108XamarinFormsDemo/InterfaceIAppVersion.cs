using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS108XamarinFormsDemo {
    public interface IAppVersion {
        string GetVersion();
        int GetBuild();
    }
}