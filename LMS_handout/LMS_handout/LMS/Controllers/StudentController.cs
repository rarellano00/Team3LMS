using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : CommonController
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            uint theID = uint.Parse(uid.Remove(0, 1));

            var query =
                from s in db.Student
                where s.UId == theID
                join e in db.Enrolled on s.UId equals e.UId
                join c in db.Class on e.ClassId equals c.ClassId
                join co in db.Course on c.CourseId equals co.CourseId
                join d in db.Department on co.DepartmentId equals d.DepartmentId
                select new
                {
                    subject = d.Abbrv,
                    number = co.Number,
                    name = co.Name,
                    season = c.Season,
                    year = c.Year,
                    grade = e.Grade ?? "--" // uses a null coalescing operator to assign -- if null grade
                };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            uint theID = uint.Parse(uid.Remove(0, 1));

            // this query will hold a students assignments and the categories those assignments belong to in a given class
            var query1 =
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
                    a.AId,
                    aname = a.Name,
                    cname = ac.Name,
                    due = a.DueDate
                };

            var query2 =
                from q in query1 // use our results from above
                join s in db.Submission
                on new { A = q.AId, B = theID } equals new { A = s.AId, B = s.UId } // left join that compares to complex objects from both queries
                into joined
                from j in joined.DefaultIfEmpty()
                select new
                {
                    aname = q.aname,
                    cname = q.cname,
                    due = q.due,
                    score = j == null ? default(uint?) : j.Score // if there is no score then default to null
                };

            return Json(query2.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            uint theID = uint.Parse(uid.Remove(0, 1));

            var query =
                from d in db.Department
                where d.Abbrv == subject
                join co in db.Course on d.DepartmentId equals co.DepartmentId
                where co.Number == num
                join cl in db.Class on co.CourseId equals cl.CourseId
                where cl.Season == season && cl.Year == year
                join ac in db.AssignmentCategory on cl.ClassId equals ac.ClassId
                where ac.Name == category
                join asg in db.Assignment on ac.AcId equals asg.AcId
                where asg.Name == asgname
                select asg;

            var query2 =
                from q in query // use our results from above
                join s in db.Submission
                on new { A = q.AId, B = theID } equals new { A = s.AId, B = s.UId } // left join that compares to complex objects from both queries
                into joined
                from j in joined.DefaultIfEmpty()
                select new
                {
                    j
                };


            if (query2.First().j != null)
            {
                query2.First().j.Contents = contents;
                query2.First().j.Time = DateTime.Now;
            }
            else
            {
                Submission newSub = new Submission();
                newSub.Time = DateTime.Now;
                newSub.Score = 0;
                newSub.Contents = contents;
                newSub.AId = query.First().AId;
                newSub.UId = theID;
                db.Submission.Add(newSub);
            }

            db.SaveChanges();


            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            uint theID = uint.Parse(uid.Remove(0, 1));

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

            Enrolled e = new Enrolled();
            e.ClassId = query.First().ClassId;
            e.UId = theID;
            e.Grade = "--";

            db.Enrolled.Add(e);

            if (db.SaveChanges() > 0)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {

            uint theID = uint.Parse(uid.Remove(0, 1));

            var query =
                from e in db.Enrolled
                where e.UId == theID
                select e;

            double total = 0;
            int classes = 0;

            foreach (var item in query)
            {
                if(item.Grade is null || item.Grade == "--")
                {
                    continue;
                }
                else
                {
                    classes++;

                    switch (item.Grade)
                    {
                        case "A":
                            total += 4;
                            break;
                        case "A-":
                            total += 3.7;
                            break;
                        case "B+":
                            total += 3.3;
                            break;
                        case "B":
                            total += 3;
                            break;
                        case "B-":
                            total += 2.7;
                            break;
                        case "C+":
                            total += 2.3;
                            break;
                        case "C":
                            total += 2;
                            break;
                        case "C-":
                            total += 1.7;
                            break;
                        case "D+":
                            total += 1.3;
                            break;
                        case "D":
                            total += 1;
                            break;
                        case "D-":
                            total += .7;
                            break;
                        case "E":
                            total += 0;
                            break;
                        default:
                            total += 0;
                            break;
                    }
                }
            }
            double gpa = 0;
            
            if(classes == 0)
            {
                return Json(gpa);
            }

            gpa = total / classes;

            return Json(new { gpa = gpa });
        }

        /*******End code to modify********/

    }
}