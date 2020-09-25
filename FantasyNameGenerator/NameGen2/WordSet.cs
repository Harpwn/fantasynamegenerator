using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyNameGenerator.NameGen2
{
	namespace Delimiters
	{
		/// <summary>
		/// Constants class for common sets of delimiters
		/// </summary>
		public static class Constants
		{
			public static string Common = " !.,-'\r\n\t";
		}
	}

	/// <summary>
	/// A utility class used for creating the Markov Chain map.
	/// </summary>
	public class WordSet
	{
		/// <summary>
		/// An object representing a chain result.
		/// </summary>
		public struct Chain
		{
			public int weight;
			public char value;
		}

		/// <summary>
		/// A dictionary containing dictionaries of chain results. The first (string) key is the 'prefix' to add onto, 
		/// the second (char) is the 'value' of the chain result. Basically, this is the graph that contains letter
		/// probabilities (by weight) used to build strings.
		/// </summary>
		private Dictionary<string, Dictionary<char, Chain>> map = new Dictionary<string, Dictionary<char, Chain>>();

		/// <summary>
		/// A hashset containing the original words used to build this WordSet. Useful for ensuring there are no 
		/// instances of recreating original words.
		/// </summary>
		private HashSet<string> sourceSet;

		/// <summary>
		/// Random number generator used to determine the next letter.
		/// </summary>
		private Random random;

		/// <summary>
		/// The number of letters to use in determining the next in a chain. An order of '2' will use substrings of length 2
		/// when storing and searching through the map.
		/// </summary>
		private int order;

		public WordSet(int order)
		{
			this.sourceSet = new HashSet<string>();
			this.random = new Random();
			this.order = order;
		}

		/// <summary>
		/// Adds a (single) word to the WordSet.
		/// </summary>
		/// <param name="word"></param>
		public void addWord(string word)
		{
			if (string.IsNullOrWhiteSpace(word))
				return;

			//If we have already visited this word, skip
			if (sourceSet.Contains(word))
				return;

			sourceSet.Add(word);

			//Add chains as substrings of length 'order' or less
			word = word.ToLower() + " ";
			string tmp = "";
			for (int i = 0; i < word.Length; i++)
			{
				if (tmp.Length > order)
					tmp = tmp.Substring(1, order);

				addChain(tmp, word[i]);
				tmp += word[i];
			}
		}

		/// <summary>
		/// Adds a set of words (from a string) using the given delimiters to seperate the words.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="delims"></param>
		public void addWords(string str, string delims)
		{
			string legalChars = "abcdefghijklmnopqrstuvwxyz ";
			str = str.ToLower();

			//Substitute all delims with space
			foreach (var c in delims)
				str = str.Replace(c, ' ');

			//Only accept the characters above
			string tmp = "";
			foreach (var c in str)
				if (legalChars.Contains(c))
					tmp += c;

			//'Flatten' the spaces in the string so we don't have empty results
			str = tmp;
			str = str.Replace("  ", " ");
			var split = str.Split(' ');

			//Add each word
			foreach (var word in split)
				addWord(word);
		}

		/// <summary>
		/// Given an existing word, attempt to find a character that would follow. A space indicates 
		/// the word should be terminated there.
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		public char next(string word)
		{
			word = word.ToLower();

			//Create the substring used to search for the next letter
			string subStr = word;
			if (word.Length > order)
				subStr = word.Substring(word.Length - order, order);

			//While there are no available options in the map, shorten the substring if possible
			while (subStr.Length > 1 && map.ContainsKey(subStr) == false)
				subStr = subStr.Substring(1, subStr.Length - 1);

			//If there are no options, even with a shortened substring, terminate the word
			if (map.ContainsKey(subStr) == false)
				return ' ';

			//Randomly determine the next letter from the possible options
			var options = map[subStr].Values.ToList();
			var total = options.Sum(c => c.weight);
			var nextIndex = random.Next(total);
			var current = 0;
			var select = ' ';

			foreach (var c in options)
			{
				current += c.weight;
				if (current > nextIndex)
				{
					select = c.value;
					break;
				}
			}

			return select;
		}

		/// <summary>
		/// Returns true if the given word was in the source set.
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		public bool containsSourceWord(string word)
		{
			if (word == null)
				return false;

			return sourceSet.Contains(word.Trim());
		}

		/// <summary>
		/// Returns true if the word has valid letter options in the chain.
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		public bool containsOptions(string word)
		{
			word = word.ToLower();

			//Create the substring used to search for the next letter
			string subStr = word;
			if (subStr.Length > order)
				subStr = word.Substring(word.Length - order, order);

			//While there are no available options in the map, shorten the substring if possible
			while ((map.ContainsKey(subStr) == false || map[subStr].Count == 0) && subStr.Length > 1)
				subStr = subStr.Substring(1, subStr.Length - 1);

			//If no options were available, skip
			if (subStr.Length == 0)
				return false;

			if (map.ContainsKey(subStr) == false)
				return false;

			//Return true if there are options available
			var set = map[subStr];
			return map[subStr].Count > 0;
		}

		/// <summary>
		/// Adds a chain to the map.
		/// </summary>
		/// <param name="pre"></param>
		/// <param name="next"></param>
		private void addChain(string pre, char next)
		{
			//Create the chain list if there isn't one for the given prefix
			if (map.ContainsKey(pre) == false)
				map.Add(pre, new Dictionary<char, Chain>());

			//Create the chain if there isn't one for the given prefix
			var list = map[pre];
			if (list.ContainsKey(next) == false)
				list.Add(next, new Chain()
				{
					value = next,
					weight = 0,
				});

			//Add one to the weight of the given chain
			var chain = list[next];
			chain.weight += 1;
			list[next] = chain;
		}
	}
}