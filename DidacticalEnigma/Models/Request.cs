﻿using DidacticalEnigma.Utils;

namespace DidacticalEnigma.Models
{
    public class Request
    {
        public string Character { get; }

        public string Word { get; }

        public string QueryText { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public Request(string character, string word, string queryText, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown)
        {
            Character = character;
            Word = word;
            QueryText = queryText;
            PartOfSpeech = partOfSpeech;
        }
    }
}