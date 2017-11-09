﻿using ColorzCore.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorzCore.Parser.AST
{
    class StatementNode// : IASTNode
    {
        public Token Raw { get; set; }
        public IList<IParamNode> Parameters { get; set; }
    }
}
