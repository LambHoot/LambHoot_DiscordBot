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
        private ActivationNetwork network;

        public NeuralNet()
        {
            Console.WriteLine("_NEURAL NET CONSTRUCTED_");
            max_sentence_length = 0;
            setUp();
            //TwitterLoop();
        }

        public void setUp()
        {
            //load exitisting network
            
            this.network = (ActivationNetwork)Network.Load("lambhoot_scripts_neural_network_-4680.bin");
            this.max_sentence_length = this.network.InputsCount;
            return;







            //read and vectorize training set
            double[][] vectorized_data = vectorize_training_file();

            //initialize input and output values using vectorized_data


            List<double[]> inputs = new List<double[]>();
            List<double[]> outputs = new List<double[]>();

            InputOutputVectorsPair[] IOVPairs = get_all_input_output_pairs_from_vector(vectorized_data);

            //count how many are non-empty
            int non_empty_count = 0;
            foreach (InputOutputVectorsPair IOVPair in IOVPairs)
            {
                if (IOVPair.inputs.Length != 0)
                    non_empty_count++;
            }

                

            //double[][] inputs = new double[non_empty_count * max_sentence_length][];
            //double[][] outputs = new double[non_empty_count * max_sentence_length][];

            int io_i = 0;
            foreach (InputOutputVectorsPair IOVPair in IOVPairs)
            {
                if (IOVPair.inputs.Length != 0)
                {
                    foreach (double[] pair_input in IOVPair.inputs)
                    {
                        if(pair_input != null)
                            inputs.Add(pair_input);
                    }
                    foreach (double[] pair_output in IOVPair.outputs)
                    {
                        if (pair_output != null)
                            outputs.Add(pair_output);
                    }
                }
                io_i++;
            }

            double[][] input = inputs.ToArray<double[]>();
            double[][] output = outputs.ToArray<double[]>();

            var s = 0;

            //should have inputs and outputs by this point

            /*
            double[][] input = new double[4][] {
                new double[] {0, 0}, new double[] {0, 1},
                new double[] {1, 0}, new double[] {1, 1}
            };
            double[][] output = new double[4][] {
                new double[] {0}, new double[] {1},
                new double[] {1}, new double[] {0}
            };
            */

            //create neural network
            ActivationNetwork network = new ActivationNetwork(
                new SigmoidFunction(0.0001),
                max_sentence_length, // max possible inputs passed to the network
                5, // three neurons in the first layer
                6, // three neurons in the first layer
                3, // three neurons in the first layer
                1); // one neuron in the final (output) layer, picks one char as output
                    // create teacher
            BackPropagationLearning teacher = new BackPropagationLearning(network);
            teacher.LearningRate = 0.5;

            //train
            Console.WriteLine("_TRAINING BEGIN_");
            bool training_complete = false;
            int training_limit = 10000;
            while (!training_complete && training_limit > 0)
            {
                training_limit--;
                // run epoch of learning procedure
                double error = teacher.RunEpoch(input, output);

                Console.WriteLine("error: " + error);
                training_complete = error < 0.1;

                if (training_limit % 10 == 0)
                {//every 1000, save the trained network
                    network.Save("lambhoot_scripts_neural_network_" + (10 - training_limit) + ".bin");
                    Console.WriteLine("NEW NEURAL NET SAVED: lambhoot_scripts_neural_network_" + (10 - training_limit) + ".bin");
                }

                // check error value to see if we need to stop
                // ...
            }

            network.Save("lambhoot_scripts_neural_network_FINAL.bin");
            //Network.Load( "lambhoot_scripts_neural_network.bin" );

            //double[] compute_input = { 1, 1 };

            //double[] computed =  network.Compute(compute_input);

            var x = 0;
        }

       

        public InputOutputVectorsPair[] get_all_input_output_pairs_from_vector(double[][] vectors)
        {

            InputOutputVectorsPair[] IOVectorPairs = new InputOutputVectorsPair[vectors.Length];

            int vi = 0;
            foreach(double[] vector in vectors)
            {
                //break down by length and create pairs of input lead and single output character
                double[][] input = new double[vector.Length][];
                double[][] output = new double[vector.Length][];
                for (int i = 0; i < vector.Length - 1; i++)
                {
                    double[] input_row = new double[max_sentence_length];
                    double[] relevant_input = vector.Take(i).ToArray();
                    for(int ri = 0; ri < relevant_input.Length; ri++)
                    {
                        input_row[ri] = relevant_input[ri];
                    }
                    input[i] = input_row;
                    //double[] output = vector.Skip(i).ToArray();
                    output[i] = new double[]{ vector[i+1] };
                }
                //insert into array of pairs
                IOVectorPairs[vi] = new InputOutputVectorsPair(input, output);
                vi++;
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
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            

            double[] sentence_vector = new double[sentence.Length * 8];
            int i = 0;
            foreach (char c in sentence)
            {
                var byte_c = Encoding.UTF8.GetBytes(c.ToString());
                string binStr = string.Join("", byte_c.Select(b => Convert.ToString(b, 2)));

                //some special characters are coming out with string lengths of 6 missing leading 0
                while(binStr.Length < 7)
                {
                    binStr = '0' + binStr;
                }

                sentence_vector[i] = 0;
                i++;
                foreach (char bit in binStr)
                {
                    if(bit == '1')
                        sentence_vector[i] = 1;
                    else
                        sentence_vector[i] = 0;
                    i++;
                }
            }
            foreach (double b in sentence_vector)
            {
                Console.Write(b);
            }
            return sentence_vector;
        }


        public string vector_to_sentence(double[] vector)
        {
            string vector_sentence = "";
            for (int i = 0; i < vector.Length; i += 8)
            {
                string byte_string = "";
                for(int j = 0; j < 8; j++)
                {
                    byte_string += vector[i + j];
                }

                var bytesAsStrings =
                byte_string.Select((c, ii) => new { Char = c, Index = ii })
                     .GroupBy(x => x.Index / 8)
                     .Select(g => new string(g.Select(x => x.Char).ToArray()));

                byte[] bytes = bytesAsStrings.Select(s => Convert.ToByte(byte_string, 2)).ToArray();


                //take a bytes array with 1 byte and convert the byte into a char
                char sentence_char = Encoding.UTF8.GetString(bytes)[0];
                vector_sentence += sentence_char;
            }
            return vector_sentence;
        }



        public string generateSentenceFrom(string begin_sentence, int char_length)
        {
            double[] sentence_vector = sentence_to_vector(begin_sentence);
            int current_char_length = sentence_vector.Length;
            double[] input_vector = new double[this.network.InputsCount];
            for (int i = 0; i < sentence_vector.Length; i++)
            {
                input_vector[i] = sentence_vector[i];
            }

            for(int i = current_char_length; i < char_length; i++)
            {
                //get next output
                double[] output = this.network.Compute(input_vector);
                double next_char_code = (double)output[0];
                input_vector[i] = Convert.ToInt32(next_char_code);
            }

            //get the sentence back
            string sentence = vector_to_sentence(input_vector);
            return sentence;
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
