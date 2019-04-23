using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            // finds classID and parses it
            int theID = -1;
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                select cl.ClassId;
            foreach (var item in query)
            {
                theID = int.Parse(item.ToString());
            }

            // finds students that are enrolled in same class ID 

            var query2 =
                from e in db.Enrolled
                where e.ClassId == theID
                join s in db.Student on e.UId equals s.UId
                select new
                {
                    fname = s.FName,
                    lname = s.LName,
                    uid = "u" + s.UId.ToString().PadLeft(7, '0'),
                    dob = s.Dob,
                    grade = e.Grade
                };


            return Json(query2.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            // case for when we just want all the assignments in a class
            if(category == null)
            {
                var nullQuery =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                join a in db.Assignment on ac.AcId equals a.AcId
                select new
                {
                    aname = a.Name,
                    cname = ac.Name,
                    due = a.DueDate,
                    submissions = a.Submission.Count
                };

                return Json(nullQuery.ToArray());

            }

            // otherwise we want all the assignments in a specified category 
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                where ac.Name == category
                join a in db.Assignment on ac.AcId equals a.AcId
                select new
                {
                    aname = a.Name,
                    cname = ac.Name,
                    due = a.DueDate,
                    submissions = a.Submission.Count
                };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                select new
                {
                    name = ac.Name,
                    weight = ac.Weight
                };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            // grab the class id
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                select new
                {
                    cl.ClassId
                };

            uint theID = 0;

            // parse the id
            foreach (var item in query)
            {
                theID = item.ClassId;
                break;
            }

            // make sure the category doesn't already exist
            if(db.AssignmentCategory.Any(checkCat => checkCat.Name == category && checkCat.ClassId == theID))
            {
                return Json(new { success = false });
            }

            // make our new category and save it
            AssignmentCategory newCat = new AssignmentCategory();
            newCat.Name = category;
            newCat.Weight = (sbyte)catweight;
            newCat.ClassId = theID;

            db.AssignmentCategory.Add(newCat);
            db.SaveChanges();


            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {

            // grab our category id
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                where ac.Name == category
                select new
                {
                    ac.AcId
                };

            // set category id to be used
            uint theID = 0;
            foreach (var item in query)
            {
                theID = item.AcId;
                break;
            }

            // make sure we don't have a duplicate assignment
            if (db.Assignment.Any(asg => asg.Name == asgname && asg.AcId == theID))
            {
                return Json(new { success = false });
            }

            // assignment creation
            Assignment a = new Assignment();
            a.Name = asgname;
            a.Points = (uint)asgpoints;
            a.DueDate = asgdue;
            a.Contents = asgcontents;
            a.AcId = theID;

            db.Assignment.Add(a);
            db.SaveChanges();




            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Year == year
                join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                where ac.Name == category
                join a in db.Assignment on ac.AcId equals a.AcId
                where a.Name == asgname
                join sub in db.Submission on a.AId equals sub.AId
                join stu in db.Student on sub.UId equals stu.UId
                select new
                {
                    fname = stu.FName,
                    lname = stu.LName,
                    uid = "u" + stu.UId.ToString().PadLeft(7,'0'),
                    time = sub.Time,
                    score = sub.Score
                };


            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            uint theID = (uint)int.Parse(uid.Remove(0, 1));

            var query = from d in db.Department
                        where d.Abbrv == subject
                        join co in db.Course on d.DepartmentId equals co.DepartmentId
                        where co.Number == num
                        join cl in db.Class on co.CourseId equals cl.CourseId
                        where cl.Year == year && cl.Season == season
                        join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                        where ac.Name == category
                        join a in db.Assignment on ac.AcId equals a.AcId
                        where a.Name == asgname
                        join s in db.Submission on a.AId equals s.AId
                        where s.UId == theID
                        select s;

            if (query.Any())
            {
                foreach (var item in query)
                {
                    item.Score = (uint)score;
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            // removes u from uid
            int theid = int.Parse(uid.Remove(0, 1));

            var query =
                from cl in db.Class
                where cl.ProfessorId == theid
                join co in db.Course on cl.CourseId equals co.CourseId
                join d in db.Department on co.DepartmentId equals d.DepartmentId
                select new
                {
                    subject = d.Abbrv,
                    number = co.Number,
                    name = co.Name,
                    season = cl.Season,
                    year = cl.Year
                };

            return Json(query.ToArray());
        }


        /*******End code to modify********/

    }
}