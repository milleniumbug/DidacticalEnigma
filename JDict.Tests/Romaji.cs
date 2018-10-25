﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using NMeCab;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class Romaji
    {
        private static IMorphologicalAnalyzer<IEntry> mecab;

        private static IKanaProperties kanaProperties;

        private static readonly TestCaseData[] TestCases =
        {
            new TestCaseData("kore wa watashi", "これは私"), 
            new TestCaseData("shounen", "少年"), 
            new TestCaseData("shoujo", "少女"),
            new TestCaseData("fushigi", "ふしぎ"),
            new TestCaseData("fushigi", "不思議"),
            new TestCaseData("senpai", "先輩"),
            // The expected of this test should actually be jun'ichirou
            // but morphological analyzer treats this as multiple words
            // spacing doesn't really matter in this case, but whether 
            // romanization can distinguish between ん (n) followed by い (i)
            // and に (ni)
            new TestCaseData("jun'ichi rou", "じゅんいちろう"), 
            new TestCaseData("ikiru ka shinu ka 、 sore ga mondai da", "生きるか死ぬか、それが問題だ")
        };

        [TestCaseSource(nameof(TestCases))]
        public void Test(string expected, string input)
        {
            var romaji = new ModifiedHepburn(mecab, kanaProperties);
            Assert.AreEqual(expected, romaji.ToRomaji(input));
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            var baseDir = TestDataPaths.BaseDir;
            mecab = new MeCabIpadic(new MeCabParam
            {
                DicDir = TestDataPaths.Ipadic,
            });
            kanaProperties = new KanaProperties2(
                TestDataPaths.Kana,
                Encoding.UTF8);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            mecab.Dispose();
        }
    }
}
