using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NEAT
{
	/// <summary>
	/// A helper class witch can save a given Genome in a file.
	/// </summary>
	public static class GenomeSaver
	{
		public static string DefaultSaveDir
		{
			get
			{
				return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\";
			}
		}

		public static string GenerateSaveFilePath(string dir, float fitness, int generation = 0)
		{
			return dir + "Generation" + generation + "F" + fitness + ".genome";
		}

		public static void SaveGenome(Genome genomeToSave, string filePath)
		{
			var bf = new BinaryFormatter();
			FileStream file = File.Create(filePath);

			bf.Serialize(file, new PackedGenome(genomeToSave));
			file.Close();
		}

		public static Genome LoadGenome(NEATConfig config, string filePath)
		{
			if (!File.Exists(filePath))
				throw new Exception(filePath + " doesn't exist.");

			var bf = new BinaryFormatter();
			FileStream file = File.Open(filePath, FileMode.Open);

			var packedGenome = (PackedGenome)bf.Deserialize(file);
			file.Close();
			return new Genome(config, packedGenome);
		}
	}
}
