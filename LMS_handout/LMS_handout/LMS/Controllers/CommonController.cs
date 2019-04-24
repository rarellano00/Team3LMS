using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        protected Team3LMSContext db;

        public CommonController()
        {
            db = new Team3LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        public void UseLMSContext(Team3LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {

            var query =
                from d in db.Department
                select new
                {
                    name = d.Name,
                    subject = d.Abbrv
                };
            return Json(query.ToArray());

        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query =
                from d in db.Department
                select new
                {
                    subject = d.Abbrv,
                    dname = d.Name,
                    courses = from c in d.Course
                              select new
                              {
                                  number = c.Number,
                                  cname = c.Name
                              }
                };

            //return Json(new[] { new { name = "None", subject = "NONE" } });

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {

            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == number
                join cl in db.Class on co.CourseId equals cl.CourseId
                select new
                {
                    season = cl.Season,
                    year = cl.Year,
                    location = cl.Location,
                    start = cl.Start,
                    end = cl.End,
                    fname = cl.Professor.FName,
                    lname = cl.Professor.LName
                };

            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            Assignment query =
                (from d in db.Department
                 where d.Abbrv == subject
                 join c in db.Course on d.DepartmentId equals c.DepartmentId
                 where c.Number == num
                 join cl in db.Class on c.CourseId equals cl.CourseId
                 where cl.Season == season && cl.Year == year
                 join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                 where ac.Name == category
                 join a in db.Assignment on ac.AcId equals a.AcId
                 where a.Name == asgname
                 select a).First();


            // TODO : returning correctly?

            return Content(query.Contents);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            uint sID = uint.Parse(uid.Remove(0, 1));

            var query =
                      from d in db.Department
                      where d.Abbrv == subject
                      join c in db.Course on d.DepartmentId equals c.DepartmentId
                      where c.Number == num
                      join cl in db.Class on c.CourseId equals cl.CourseId
                      where cl.Season == season && cl.Year == year
                      join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                      where ac.Name == category
                      join a in db.Assignment on ac.AcId equals a.AcId
                      where a.Name == asgname
                      join s in db.Submission on a.AId equals s.AId
                      where s.UId == sID
                      select s;

            if (query.Any())
            {
                return Content(query.First().Contents);
            }

            return Content("");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            /* TODO : 
             * Note this is a pretty brute force solution, maybe should try to optimize later.
             */


            uint theID = uint.Parse(uid.Remove(0, 1));

            var squery =
                from s in db.Student
                join d in db.Department on s.DepartmentId equals d.DepartmentId
                where s.UId == theID
                select new { fname = s.FName, lname = s.LName, uid = s.UId, department = d.Name };
            var pquery =
                from p in db.Professor
                where p.UId == theID
                join d in db.Department on p.DepartmentId equals d.DepartmentId
                select new { fname = p.FName, lname = p.LName, uid = p.UId, department = d.Name };
            var aquery =
                from a in db.Administrator
                where a.UId == theID
                select a;

            if (squery.Any())
            {
                foreach (var item in squery)
                {
                    return Json(new { fname = item.fname, lname = item.lname, uid = "u" + item.uid.ToString().PadLeft(7, '0'), department = item.department });
                }
            }
            else if (pquery.Any())
            {
                foreach (var item in pquery)
                {
                    return Json(new { fname = item.fname, lname = item.lname, uid = "u" + item.uid.ToString().PadLeft(7, '0'), department = item.department });
                }
            }
            else
            {
                foreach (var item in aquery)
                {
                    return Json(new { fname = item.FName, lname = item.LName, uid = "u" + item.UId.ToString().PadLeft(7, '0') });

                }
            }

            return Json(new { success = false });
        }


        /******End code to modify********/
    }
}