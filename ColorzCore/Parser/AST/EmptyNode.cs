﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorzCore.Parser.AST
{
    class EmptyNode : IASTNode, ILineNode, IAtomNode
    {
        public int Precedence => throw new NotImplementedException();

        public ParamType Type => throw new NotImplementedException();

        public int Evaluate()
        {
            throw new NotImplementedException();
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
