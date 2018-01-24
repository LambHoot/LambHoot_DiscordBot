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

        public NeuralNet()
        {
            Console.WriteLine("_NEURAL NET CONSTRUCTED_");
            setUp();
            //TwitterLoop();
        }

        public void setUp()
        {
            //read and vectorize training set



            // create neural network
            // initialize input and output values
            double[][] input = new double[4][] {
                new double[] {0, 0}, new double[] {0, 1},
                new double[] {1, 0}, new double[] {1, 1}
            };
                        double[][] output = new double[4][] {
                new double[] {0}, new double[] {1},
                new double[] {1}, new double[] {0}
            };
            // create neural network
            ActivationNetwork network = new ActivationNetwork(
                new SigmoidFunction(2),
                2, // two inputs in the network
                3, // two neurons in the first layer
                1); // one neuron in the second layer (output)
                    // create teacher
            BackPropagationLearning teacher =
                new BackPropagationLearning(network);
            // loop


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

       



        //train neural net


        public string generateSentence()
        {

            return "a sentence.";
        }

    }
}
