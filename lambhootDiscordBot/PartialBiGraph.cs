using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lambhootDiscordBot
{
    class PartialBiGraph
    {
        public Dictionary<string, Word> wordDictionary = new Dictionary<string, Word>();


    }










    #region Word Class
    public class Word
    {
        public string wordString;
        public List<string> wordAfterList;//list of all words appearing immediately after this word
        public List<int> wordAfterCountList;//counts for the number of instances of the words in wordAfterList
        public List<float> wordAfterProbList;//probabilities for each of the words in wordAfterList (value between 0/1)

        public Word(string wordString)
        {
            this.wordString = wordString;
            wordAfterList = new List<string>();
            wordAfterCountList = new List<int>();
            wordAfterProbList = new List<float>();
        }

        public void addWordAfter(string wordAfter)
        {
            int index = wordAfterList.IndexOf(wordAfter);
            if (index != -1)//wordAfter already stored
            {
                wordAfterCountList[index] += 1;//increase the count
            }
            else//wordAfter is new
            {
                wordAfterList.Add(wordAfter);
                wordAfterCountList.Add(1);
            }
        }

        //is called when all wordAfters are added and the wordAfterProbList is still empty
        public void processWordAfterProbabilities()
        {
            float totalWordAfterCount = wordAfterCountList.Sum();
            foreach (int count in wordAfterCountList)
                wordAfterProbList.Add((float)count/totalWordAfterCount);
        }

    }
    #endregion Word Class

}
