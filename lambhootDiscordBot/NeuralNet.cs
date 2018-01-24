using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using AForge.Neuro;
using AForge.Neuro.Learning;
using AForge.Math;
using AForge.Math.Random;

namespace lambhootDiscordBot
{
    class NeuralNet
    {
        private System.IO.StreamReader file;
        private string trainingFilePath;
        private int max_sentence_length = 0;

        public NeuralNet()
        {
            Console.WriteLine("_NEURAL NET CONSTRUCTED_");
            max_sentence_length = 0;
            setUp();
            //TwitterLoop();
        }

        public void setUp()
        {
            //read and vectorize training set
            double[][] vectorized_data = vectorize_training_file();

            //initialize input and output values using vectorized_data
            double[][] input = new double[4][] {
                new double[] {0, 0}, new double[] {0, 1},
                new double[] {1, 0}, new double[] {1, 1}
            };
            double[][] output = new double[4][] {
                new double[] {0}, new double[] {1},
                new double[] {1}, new double[] {0}
            };

            //create neural network
            ActivationNetwork network = new ActivationNetwork(
                new SigmoidFunction(2),
                max_sentence_length, // max possible inputs passed to the network
                3, // three neurons in the first layer
                max_sentence_length); // one neuron in the final (output) layer
                    // create teacher
            BackPropagationLearning teacher =
                new BackPropagationLearning(network);

            //train
            bool training_complete = false;
            int training_limit = 10000;
            while (!training_complete && training_limit > 0)
            {
                training_limit--;
                // run epoch of learning procedure
                double error = teacher.RunEpoch(input, output);

                Console.WriteLine("error: " + error);
                training_complete = error < 0.1;

                // check error value to see if we need to stop
                // ...
            }

            double[] compute_input = { 1, 1 };

            double[] computed =  network.Compute(compute_input);

            var x = 0;
        }

       

        public double[][] get_all_input_output_pairs_from_vector(double[][] vectors)
        {
            //TODO if vectors is null, skip it

            InputOutputVectorsPair[] IOVectorPairs = new InputOutputVectorsPair[vectors.Length];

            foreach(double[] vector in vectors)
            {
                //break down by length and create pairs
                //insert into array of pairs
            }
            return IOVectorPairs;
        }

        public double[][] vectorize_training_file()
        {
            Console.WriteLine("Training filepath: ");
            trainingFilePath = @"" + Console.ReadLine();
            file = new System.IO.StreamReader(trainingFilePath);

            //C:\Users\Denis\Documents\Visual Studio 2015\Projects\lambhootDiscordBot\twitter\LH_Scripts.txt

            Stack<string> sentences = new Stack<string>();
            string readLine;
            while ((readLine = file.ReadLine()) != null)
            {
                if(readLine.Length > max_sentence_length)
                    max_sentence_length = readLine.Length;
                sentences.Push(readLine);
            }
            //build vector of vectors
            double[][] sentence_vectors = new double[sentences.Count][];

            for(int i = 0; i < sentence_vectors.Count(); i++)
            {
                sentence_vectors[i] = sentence_to_vector(sentences.Pop());
            }
            file.Close();
            return sentence_vectors;
        }


        public double[] sentence_to_vector(string sentence)
        {
            double[] sentence_vector = new double[sentence.Length];
            int i = 0;
            foreach (char c in sentence)
            {
                sentence_vector[i] = (int)(decimal)c;
                i++;
            }
            return sentence_vector;
        }


        public string vector_to_sentence(double[] vector)
        {
            string vector_sentence = "";
            foreach (int code in vector)
            {
                vector_sentence += char.ConvertFromUtf32(code);
            }
            return vector_sentence;
        }



        public string generateSentence()
        {

            return "a sentence.";
        }

    }

    public class InputOutputVectorsPair
    {
        public double[][] inputs;
        public double[][] outputs;

        public InputOutputVectorsPair(double[][] passed_inputs, double[][] passed_outputs)
        {
            this.inputs = passed_inputs;
            this.outputs = passed_outputs;
        }

    }
}
