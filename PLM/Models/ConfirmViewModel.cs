using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;

namespace PLM.Models
{
    public class ConfirmViewModel
    {
        /// <summary>
        /// Base 64 image data
        /// </summary>
        public string newBase64ImageData { get; set; }
        /// <summary>
        /// The original image's url
        /// </summary>
        public string originalBase64ImageData { get; set; }
        /// <summary>
        /// The ID of the image in the database
        /// </summary>
        public string imageID { get; set; }
        /// <summary>
        /// The ID of the answer in the database
        /// </summary>
        public string answerID { get; set; }

        public ConfirmViewModel(string new64, string original64, string imgID, string ansID)
        {
            newBase64ImageData = new64;
            originalBase64ImageData = original64;
            imageID = imgID;
            answerID = ansID;
        }
    }
}