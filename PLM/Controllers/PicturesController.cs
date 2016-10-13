using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PLM;
using System.IO;
using PLM.Extensions;
using PLM.CutomAttributes;
using System.Drawing;
using System.Drawing.Imaging;
using PLM.Models;

namespace PLM.Controllers
{
    public class PicturesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool incorrectImageType = false;
        private bool imageSizeTooLarge = false;
        private Picture pictureToSave;
        
        public ActionResult Index()
        {
            //ConvertAllPicturesToStringData();
            var pictures = db.Pictures.Include(p => p.Answer);
            return View(pictures.ToList());
        }
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            return View(picture);
        }

        public ActionResult PictureView(int? id)
        {
            Picture picture = db.Pictures.Find(id);
            PictureToView pic = new PictureToView(picture);
            return View(pic);
        }

        public ActionResult PictureViewImageEditor(int? id)
        {
            Picture picture = db.Pictures.Find(id);
            return View(picture);
        }

        //Convert all pictures saved in file
        //system to image data in database
        //public void ConvertAllPicturesToStringData()
        //{
        //    Image image;
        //    Picture picToChange;
        //    var pictures = db.Pictures.Where(p => p.PictureData == null).ToList();

        //    foreach (Picture pic in pictures)
        //    {
        //        picToChange = db.Pictures.Find(pic.PictureID);
        //        image = Image.FromFile("C:\\" + pic.Location.Replace("/", "\\").ToString());

        //        MemoryStream ms = new MemoryStream();
        //        image.Save(ms, ImageFormat.Png);
        //        byte[] imgArr = ms.ToArray();

        //        picToChange.PictureData = Convert.ToBase64String(imgArr);

        //        db.SaveChanges();
        //    }
        //}

        // GET: /Pictures/Create

        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Create(int? id)
        {
            ViewBag.AnswerID = id;
            Picture picture = new Picture();
            picture.Answer = db.Answers
                    .Where(a => a.AnswerID == id)
                    .ToList().First();

            return View(picture);
        }

        // POST: /Pictures/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Create([Bind(Include = "Attribution,PictureID")] Picture picture, int? id)
        {
            pictureToSave = new Picture();
            pictureToSave.AnswerID = (int)id;
            pictureToSave.Location = "NotYetConstructed";
            if (picture.Attribution == null)
                pictureToSave.Attribution = "";
            else
                pictureToSave.Attribution = picture.Attribution;
            ConvertImageToDataString(pictureToSave, Request.Files[0].InputStream);
            
            ViewBag.AnswerID = id;
            if (ModelState.IsValid)
            {
                db.Pictures.Add(pictureToSave);
                db.SaveChanges();
                return RedirectToAction("edit", new { controller = "Answers", id = pictureToSave.AnswerID });
            }

            ViewBag.AnswerID = new SelectList(db.Answers, "AnswerID", "AnswerString", pictureToSave.AnswerID);
            return View(pictureToSave);
        }

        [NonAction]
        public static void ConvertImageToDataString(Picture pictureToSave, Stream stream)
        {
            ImageConverter IC = new ImageConverter();
            Image image = Image.FromStream(stream);
            byte[] imageArray = (byte[])IC.ConvertTo(image, typeof(byte[]));
            pictureToSave.PictureData = Convert.ToBase64String(imageArray);
        }

        ///// <summary>
        ///// Save a picture to the server. Returns the relative path if successful, otherwise returns "FAILED"
        ///// </summary>
        ///// <param name="picture">The picture object to be saved</param>
        ///// <returns>string</returns>
        //[NonAction]
        //public string SaveUploadedFile(Picture picture, int id)
        //{
        //    imageSizeTooLarge = false;
        //    incorrectImageType = false;
        //    bool isSavedSuccessfully = false;
        //    int picCount;
        //    string answerString;

        //    using (ApplicationDbContext db2 = new ApplicationDbContext())
        //    {
        //        Session["upload"] = db2.Answers.Find(id).Module.GetModuleDirectory();
        //        answerString = db2.Answers.Find(id).AnswerString;
        //        picCount = db2.Answers.Find(id).PictureCount;
        //        picCount++;
        //    }
        //    string fName = "";
        //    string relpath = "NO FILE UPLOADED";

        //    foreach (string fileName in Request.Files)
        //    {
        //        HttpPostedFileBase file = Request.Files[fileName];
        //        fName = file.FileName;
        //        if (file.ContentLength >= 200000)
        //        {
        //            //File To Big
        //            imageSizeTooLarge = true;
        //            isSavedSuccessfully = false;
        //        }
        //        else if (file.ContentType.IndexOf("image") == -1)
        //        {
        //            //File Not An Image
        //            incorrectImageType = true;
        //            isSavedSuccessfully = false;
        //        }
        //        else
        //        {
        //            if (file != null && file.ContentLength > 0)
        //            {
        //                string moduleDirectory = (DevPro.baseFileDirectory + "PLM/" + Session["upload"].ToString() + "/");
        //                string newfName = (answerString + "-" + picCount.ToString() + ".png");
        //                relpath = (moduleDirectory + newfName);
        //                file.SaveAs(relpath);
        //                isSavedSuccessfully = true;
        //            }
        //        }
        //    }

        //    if (isSavedSuccessfully)
        //    {
        //        using (ApplicationDbContext db3 = new ApplicationDbContext())
        //        {
        //            db3.Answers.Find(id).PictureCount++;
        //            db3.Entry(db3.Answers.Find(id)).State = EntityState.Modified;
        //            db3.SaveChanges();
        //        }
        //        return relpath;
        //    }
        //    else
        //    {
        //        return "FAILED";
        //    }
        //}

        public ActionResult InvalidImage(int id)
        {
            ViewBag.AnswerID = id;
            return View();
        }

        public ActionResult FileToLarge(int id)
        {
            ViewBag.AnswerID = id;
            return View();
        }

        public ActionResult UploadError(int id)
        {
            ViewBag.AnswerID = id;
            return View();
        }

        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            ViewBag.AnswerID = new SelectList(db.Answers, "AnswerID", "AnswerString", picture.AnswerID);
            return View(picture);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Edit([Bind(Include = "PictureID,Location,AnswerID,Attribution")] Picture picture)
        {
            if (ModelState.IsValid)
            {
                db.Entry(picture).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("edit", new { controller = "Answers", id = picture.AnswerID });
            }
            ViewBag.AnswerID = new SelectList(db.Answers, "AnswerID", "AnswerString", picture.AnswerID);
            return View(picture);
        }
        
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            return View(picture);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult DeleteConfirmed(int id)
        {
            Picture picture = db.Pictures.Find(id);
            db.Pictures.Remove(picture);
            db.SaveChanges();
            return RedirectToAction("edit", new { controller = "Answers", id = picture.AnswerID });
        }

        #region From Image Editor
        [HttpGet]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult ImageEditor(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            return View(picture);
        }

        [HttpPost]
        [ActionName("ImageEditor")]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult ImageEditorPOST()
        {
            //string imgId = Request.Form.Get("imgId");
            //string answerId = Request.Form.Get("answerId");
            //string imgData = Request.Form.Get("imgData");

            //string result = SaveImage(imgData, imgId, answerId);

            //if (result == "FAILED")
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
            //            "Something went wrong with your request. \nContact an administrator.");
            //}

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Confirm()
        {
            ConfirmViewModel model = (ConfirmViewModel)TempData["model"];

            //Disallows using back button to return to page after saving or discarding. Does not permit caching.
            //Taken from Kornel at http://stackoverflow.com/questions/49547/making-sure-a-web-page-is-not-cached-across-all-browsers
            Response.Cache.SetCacheability(HttpCacheability.NoCache);  // HTTP 1.1.
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AppendHeader("Expires", "0"); // Proxies.

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        [ActionName("Confirm")]
        public ActionResult ConfirmPOST()
        {
            string b64Img = Request.Form.Get("imageData");
            string origUrl = Request.Form.Get("origUrl");
            string imgID = Request.Form.Get("imageId");
            string answerID = Request.Form.Get("answerId");
            string tempUrl = Request.Form.Get("tempUrl");
            ConfirmViewModel model = new ConfirmViewModel(b64Img, origUrl, imgID, answerID);
            TempData["model"] = model;
            return RedirectToAction("Confirm");
        }

        [HttpPost]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Save()
        {
            string origUrl = Request.Form.Get("origUrl");
            //origUrl = HttpUtility.HtmlDecode(origUrl);
            string tempUrl = Request.Form.Get("tempUrl");
            //tempUrl = HttpUtility.HtmlDecode(tempUrl);
            //string temporaryFileName = Path.GetFileName(tempUrl);
            string ansId = Request.Form.Get("answerID");
            //string result = PermaSave(temporaryFileName, origUrl);
            string imgID = Request.Form.Get("imageID");
            int imageID = Int32.Parse(imgID);

            tempUrl = tempUrl.Replace("data:image/png;base64,", "");
            tempUrl = tempUrl.Replace("data:image/jpeg;base64,", "");

            Picture picture = db.Pictures.Find(imageID);
            picture.PictureData = tempUrl;
            db.Entry(picture).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Edit", "Answers", new { id = ansId });
        }

        [HttpPost]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Discard()
        {
            string tempUrl = Request.Form.Get("tempUrl");
            tempUrl = HttpUtility.HtmlDecode(tempUrl);
            string temporaryFileName = Path.GetFileName(tempUrl);

            DiscardChanges(temporaryFileName);

            string ansId = Request.Form.Get("answerID");

            return RedirectToAction("Edit", "Answers", new { id = ansId });
        }

        /// <summary>
        /// Saves an image from data obtained from a POST.
        /// Returns the filepath if it succeeds.
        /// Returns "FAILED" if there is an exception, or the specified file format isn't supported.
        /// Returns "TOO LARGE" if the image size is greater that 200000 bytes (200KB).
        /// </summary>
        /// <param name="fromPost">The POST data in the following format: 
        /// "data:image/[FILEEXTENSION];base64,[IMAGEDATA]",
        /// where [FILEEXTENSION] is either "jpeg" or "png", and 
        /// [IMAGEDATA] is an image in Base64 encoding.</param>
        /// <param name="imgId">The id of the image that was edited. 
        /// Will be used to discriminate which image to overwrite.</param>
        /// <param name="answerId">The id of the answer. Used to select which answer the image belongs to.</param>
        /// <returns>string</returns>
        /// 
        [NonAction]
        private string SaveImage(string imageData, string imageId, string answerId)
        {
            try
            {
                string imageBase64 = imageData;
                string imageFormat = imageBase64.Substring(imageBase64.IndexOf('/') + 1, imageBase64.IndexOf(';') - 11);
                imageBase64 = imageBase64.Substring(imageBase64.LastIndexOf(',') + 1);

                byte[] img = Convert.FromBase64String(imageBase64);

                Guid g = Guid.NewGuid();
                string TempFileName = Convert.ToBase64String(g.ToByteArray());

                TempFileName = TempFileName.Replace("=", "");
                TempFileName = TempFileName.Replace("+", "");
                //TempFileName = TempFileName.Replace(@"/", "");

                TempFileName = "{" + imageId + "}" + TempFileName;
                TempFileName = "[" + answerId + "]" + TempFileName;
                TempFileName = TempFileName + "." + imageFormat;

                using (MemoryStream ms = new MemoryStream(img, 0, img.Length))
                {
                    Image image;
                    image = Image.FromStream(ms, true);

                    if (imageFormat == "jpg")
                    {
                        //image.Save(dirPath + TempFileName, ImageFormat.Jpeg);
                        image.Dispose();
                    }
                    else if (imageFormat == "png")
                    {
                        //image.Save(dirPath + TempFileName, ImageFormat.Png);
                        image.Dispose();
                    }
                    else
                    {
                        return "FAILED";
                    }
                }
                return /*dirPath + */ TempFileName;
            }
            catch (Exception)
            {
                return "FAILED";
            }
        }

        /// <summary>
        /// Permanently save the file with the given name in the tempUpload folder
        /// to a different folder, with a new name. Overwrites files with the same name that are already there.
        /// If there are multiple files found with the same name for whatever reason,
        /// takes the last one found.
        /// Returns "SAVED" if successful, 
        /// "BAD LOCATION" if the file could not be found within the temp folder 
        /// (possibly indicating that the temp folder is missing),
        /// "NO FILES FOUND" if GetFiles failed to match,
        /// "BAD MOVE ON RENAME" if TryRenameFile fails,
        /// "BAD MOVE DURING TRANSFER" if the filesMove array is nulled,
        /// or "FAILED ON FILE MOVE" otherwise.
        /// </summary>
        /// <param name="filename">The file to permanently save from the tempUpload folder. Expects only 
        /// the filename and its extension, not a path</param>
        /// <param name="toNewFilePath">The new name for the saved file, with the new path. 
        /// Expects the full path</param>
        /// <returns>string</returns>
        //[NonAction]
        //private string PermaSave(string filename, string toNewFilePath)
        //{

        //    List<string> filesToMove = new List<string>();

        //    string dirPath = (Path.Combine(Server.MapPath("~/Content/Images/tempUploads/")));
        //    //string dirPath = DevPro.baseFileDirectory + "tempUploads";
        //    string newDirPath = Path.GetDirectoryName(Server.MapPath(toNewFilePath)) + @"\";
        //    //string newDirPath = (Path.Combine(Server.MapPath("~/Content/Images/permUploads/")));
        //    string newFileName = Path.GetFileNameWithoutExtension(toNewFilePath);
        //    string result;

        //    //if the selected file doesn't exist in the temp folder
        //    if (!(System.IO.File.Exists(dirPath + filename)))
        //    {
        //        //If this code is reached, the passed in filename could not be accessed. 
        //        //It may have been moved, deleted, or did not exist in the first place.
        //        //The passed in filename may have also contained illegal characters, 
        //        //referenced a location on a failing/missing disk, 
        //        //or the program might not have read permissions for that specific file.
        //        result = "BAD LOCATION:" + dirPath + filename;
        //        return result;
        //    }

        //    string[] filesToSave = Directory.GetFiles(dirPath, filename);

        //    if (filesToSave.Length == 0)
        //    {
        //        //If this code is reached, GetFiles didn't find any matching files.
        //        return "NO FILES FOUND";
        //    }
        //    //for each file to be saved, 
        //    foreach (string filePath in filesToSave)
        //    {
        //        string newFilePath;
        //        if (FileManipExtensions.TryRenameFile(filePath, newFileName, out newFilePath, true))
        //        {
        //            filesToMove.Add(newFilePath);
        //        }
        //    }
        //    //Make sure that filepaths were actually moved to the filesToMove list.
        //    if (filesToMove.Count == 0)
        //    {
        //        //If this code is reached, there is a problem within the TryRenameFile method.
        //        //Another process may have been accessing the file at the time of the renaming.
        //        return "BAD MOVE ON RENAME";
        //    }

        //    string[] filesMove = filesToMove.ToArray();
        //    //Verify that all the filepaths were moved to the array intact
        //    //if filesMove.Length is either zero or unequal to filesToMove.Count
        //    if (filesMove.Length == 0 || filesMove.Length != filesToMove.Count)
        //    {
        //        //If this code is reached, something happened that nulled 
        //        //or removed some or all elements from the filesMove array
        //        return "BAD MOVE DURING TRANSFER";
        //    }
        //    if (FileManipExtensions.MoveSpecificFiles(filesMove, newDirPath, true))
        //    {
        //        return "SAVED";
        //    }
        //    else return "FAILED ON FILE MOVE";
        //}

        /// <summary>
        /// Discard all the images in the temp folder with the given filename.
        /// Returns "DONE" if successful, "ERROR" if the deletion fails at any point.
        /// </summary>
        /// <param name="filename">The name of the file to delete</param>
        /// <returns>string</returns>
        [NonAction]
        private string DiscardChanges(string filename)
        {
            string dirPath = (Path.Combine(Server.MapPath("~/Content/Images/tempUploads/")));
            string[] filesToDiscard = Directory.GetFiles(dirPath, filename);
            if (FileManipExtensions.DeleteSpecificFiles(filesToDiscard))
            {
                return "DONE";
            }
            else return "ERROR";
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}