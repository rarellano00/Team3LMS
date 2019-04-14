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

      //return Json(new[] { new { name = "None", subject = "NONE" } });

            
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
                from c in db.Class
                select new
                {
                    season = c.Season,
                    year = c.Year,
                    location = c.Location,
                    start = c.Start,
                    end = c.End,
                    fname = c.Professor.FName,
                    lname = c.Professor.LName
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
                select a.Contents;
            

            // TODO : returning correctly?

      return Content(query.ToString());
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
            uint sID = (uint)int.Parse(uid.Remove(0, 1));

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
                      select s.Contents;

            // TODO : correct return statement?

            return Content(query.ToString());
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
            uint theID = (uint)int.Parse(uid.Remove(0, 1));

            //check if user exists
            var query =
                (from a in db.Administrator
                 where a.UId == theID
                 select a.UId).Union
                 (from p in db.Professor
                  where p.UId == theID
                  select p.UId).Union
                 (from s in db.Student
                  where s.UId == theID
                  select s.UId);

            // TODO : how do i join with departments????

            if(!query.Any())
            {
                return Json(new { success = false });
            }
            else
            {

            }

            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}