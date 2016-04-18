using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.IO;
using System.Threading.Tasks;
using System.Net; 

namespace TestBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        string SubscriptionKey = "d7fc2c95854542acb7ff3fd7c68e94a1"; //debug only!!! 
        private async Task<AnalysisResult> UploadAndAnalyzeImage(string imageFilePath)
        {
            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE STARTS HERE
            // -----------------------------------------------------------------------

            //
            // Create Project Oxford Vision API Service client
            //
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);

            using (Stream imageFileStream = File.OpenRead(imageFilePath))
            {
                //
                // Analyze the image for all visual features
                //
                
                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                return analysisResult;
            }

            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE ENDS HERE
            // -----------------------------------------------------------------------
        }
        private async Task<AnalysisResult> AnalyzeUrl(string imageUrl)
        {
            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE STARTS HERE
            // -----------------------------------------------------------------------

            //
            // Create Project Oxford Vision API Service client
            //
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);
           

            //
            // Analyze the url for all visual features
            //
            
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageUrl, visualFeatures);
            return analysisResult;

            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE ENDS HERE
            // -----------------------------------------------------------------------
        }

        private async Task<AnalysisResult> AnalyzeLocalUrl(string imageUrl)
        {
            //not sure how this will work outside the sandbox but it works in the sandbox 

            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);

            byte[] imageData = null;

            using (var wc = new WebClient()) imageData = wc.DownloadData(imageUrl);
            Stream URLStream = new MemoryStream(imageData);
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(URLStream, visualFeatures);
            return analysisResult;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                
                //if (message.Text == string.Empty | message.Attachments.Count == 0  )
                //{
                //    return message.CreateReplyMessage("Did you say something??");
                //}
                if (message.Text.Contains(" cat?\\s"))
                {
                    return message.CreateReplyMessage("Did you say something about cats!! I love cats");
                }
                else if (message.Attachments.Count > 0 )
                {

                    return message.CreateReplyMessage(CheckForDog(message));
                }
                // return our reply to the user
                return message.CreateReplyMessage($"You sent " + message.Text.Length +" characters");
            }

            else
            {
                return HandleSystemMessage(message);
            }
        }
        
        private string CheckForDog(Message message)
        {
            string retMessageString = "";
            int numImages = 1;
            bool DogFound = false; 
            foreach (Attachment attachment  in message.Attachments )
            {
                
                if (attachment.ContentType.Contains("image")  == true ) 
                {

                    //retMessageString =  (retMessageString + "Image " + numImages + "Found::  " + attachment.ContentType + "\n");

                    AnalysisResult analysisResult;

                    var task = Task.Run(async () => await AnalyzeLocalUrl(attachment.ContentUrl));
                    task.Wait();
                    analysisResult = task.Result;
                    
                    foreach (var item in analysisResult.Tags)
                    {
                        //retMessageString = (retMessageString + "   Tag Name  : " + item.Name + "   Confidance: " + item.Confidence + "\n ");

                        if (item.Name.Contains("dog") & (item.Confidence > 0.95))
                        {
                            retMessageString = (retMessageString + "HEY I FOUND A DOG!!!!! \n");
                            DogFound = true;  
                        }
                        else if (item.Name.Contains("dog") & item.Confidence > 0.85 )
                        {
                            retMessageString = (retMessageString + "I really hope that is a dog, or a cat!  But I really want it to be a dog. \n");
                        }
                    }

                    ++numImages;

                    if (DogFound == false)
                    {
                        retMessageString =  ("Why would you not send a picture of a dog."); 
                    }
                
                }
            }
            return retMessageString; 
            
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}