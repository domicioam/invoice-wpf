﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class Icms102 : IcmsSn
    {
        public Icms102(CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}