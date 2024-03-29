﻿using PasswordKeeper.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordKeeper.Core.Utility
{
    class PasswordGenerator
    {
        private Random _rand = new Random();

        // Различные наборы символов для генерации.
        private const string Nums = "1234567890";
        private const string LowerL = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperL = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Symb = "-_";

        public string Generate(int passwordLength, PasswordTypeModel passwordType)
        {
            string password = "";

            var symbolsQueue = new List<char>();
            var symbolsPool = new List<string>() { LowerL };

            TryAddSymbolType(passwordType.IsNumberRequired, symbolsPool, Nums, symbolsQueue);
            TryAddSymbolType(passwordType.IsUpperRequired, symbolsPool, UpperL, symbolsQueue);
            TryAddSymbolType(passwordType.IsSymbolRequired, symbolsPool, Symb, symbolsQueue);

            symbolsQueue.Add(LowerL[_rand.Next(0, LowerL.Length)]);

            // Алгоритм случайной генерации пароля в соответствии с условиями.
            for (int i = 0; i < passwordLength; i++)
            {
                if (symbolsQueue.Count == 0)
                {
                    var randomSymbolsPool = symbolsPool[_rand.Next(0, symbolsPool.Count)];
                    password += randomSymbolsPool[_rand.Next(0, randomSymbolsPool.Length)];

                    continue;
                }

                var symbolIndex = _rand.Next(0, symbolsQueue.Count);
                password += symbolsQueue[symbolIndex];
                symbolsQueue.RemoveAt(symbolIndex);
            }

            return password;
        }

        private void TryAddSymbolType(bool symbolTypeFlag, List<string> symbolsPool, string symbolsType, List<char> symbolsQueue)
        {
            if (symbolTypeFlag)
            {
                symbolsQueue.Add(symbolsType[_rand.Next(0, symbolsType.Length)]);
                symbolsPool.Add(symbolsType);
            }
        }
    }
}
