﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkimiaOS.Core.Reflection.Cecil
{
    public interface IReflector
    {
        IAssemblyReflector LoadAssembly(string path);
    }
}
