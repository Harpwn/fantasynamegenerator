using System;

namespace FantasyNameGenerator.NameGen2
{
    public class MarkovChain
    {
        /// <summary>
        /// Generates a word from the given wordset and size parameters.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static string Generate(WordSet set, int minSize, int maxSize)
        {
            string word = "";

            //Loop until we find a fit
            while (true)
            {
                //Get the next letter
                var next = set.next(word);

                //If the word is too large, restart
                if (word.Length + 1 > maxSize)
                    word = "";

                //If the word is about to terminate
                if (next == ' ')
                {
                    //If the word is too small, retry
                    if (word.Length < minSize)
                    {
                        word = "";
                        continue;
                    }

                    //If the result word already exists in the wordset's source, skip
                    if (set.containsSourceWord(word))
                    {
                        word = "";
                        continue;
                    }

                    break;
                }

                //Append the letter to the word
                word += next;
            }

            return word.Trim();
        }

        /// <summary>
        /// "Mutates" an existing word by selecting an existing (random length) substring
        /// and generating a new word from the substring base.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="wordSrc"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static string Mutate(WordSet set, string wordSrc, int minSize, int maxSize)
        {
            return Mutate(set, wordSrc, minSize, maxSize, 1);
        }

        /// <summary>
        /// "Mutates" an existing word by selecting an existing (random length) substring
        /// and generating a new word from the substring base.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="wordSrc"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <param name="minRand"></param>
        /// <returns></returns>
        public static string Mutate(WordSet set, string wordSrc, int minSize, int maxSize, int minRand)
        {
            Random r = new Random(Environment.TickCount);
            if (minRand > wordSrc.Length)
                minRand = wordSrc.Length - 1;

            //Create a substring of random length from the beginning
            string subStr = wordSrc.Substring(0, r.Next(wordSrc.Length - minRand) + minRand);

            //Loop until we find a word that fits
            string word = subStr;
            while (true)
            {

                //If the word is too long, restart
                if (word.Length + 1 > maxSize)
                {

                    //If we're stuck in a loop with the original, return nothing
                    if (word == subStr)
                        return "";

                    word = subStr;
                    continue;
                }

                //If there are no available options for the current word, return nothing
                //since it is not a naturally ending word
                if (set.containsOptions(word) == false)
                    return "";

                //Get the next letter
                var next = set.next(word);

                //If the word is about to terminate
                if (next == ' ')
                {
                    //If the word is too short, retry
                    if (word.Length < minSize)
                    {
                        //If we're stuck in a loop with the original, return nothing
                        if (word == subStr)
                            return "";

                        word = subStr;
                        continue;
                    }

                    //If the result word already exists in the wordset's source, skip
                    if (set.containsSourceWord(word))
                    {
                        word = subStr;
                        continue;
                    }

                    break;
                }

                //Append the new letter to the word
                word += next;
            }

            return word.Trim();
        }
    }
}

