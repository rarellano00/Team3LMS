﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join c in db.Course on d.DepartmentId equals c.DepartmentId
                select new
                {
                    name = c.Name,
                    number = c.Number
                };


            return Json(query.ToArray());
        }





        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query =
                from d in db.Department
                where d.Abbrv == subject
                join p in db.Professor on d.DepartmentId equals p.DepartmentId
                select new
                {
                    lname = p.LName,
                    fname = p.FName,
                    uid = "u" + p.UId.ToString().PadLeft(7, '0')
                };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {

            Course c = new Course();
            c.Number = (short)number;
            c.Name = name;
            var query =
                (from d in db.Department
                 where d.Abbrv == subject
                 select d).First();
            /*
            foreach (var item in query)
            {
                depId = int.Parse(item.ToString());
            }
            */
            uint depId = query.DepartmentId;

            c.DepartmentId = depId;

            if(db.Course.Where(co=> co.Number == number && co.DepartmentId == depId).Any())
            {
                return Json(new { success = false });
            }

            // need to detect if was added or not 
            db.Course.Add(c);
            db.SaveChanges();
            if (c.CourseId < 0)
            {
                return Json(new { success = false });
            }



            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            if (db.Class.Where(cl => cl.Season == season && cl.Year == year && cl.Course.Number == number).Any())
            {
                return Json(new { success = false });
            }

            if (db.Class.Where(cl => cl.Location == location &&
            ((start.TimeOfDay.CompareTo(cl.End) <= 0 && end.TimeOfDay.CompareTo(cl.End) >= 0) || (start.TimeOfDay.CompareTo(cl.Start) <= 0 && end.TimeOfDay.CompareTo(cl.Start) >= 0))).Any())
            {
                return Json(new { success = false });
            }


            Class c = new Class();
            c.Season = season;
            c.Start = start.TimeOfDay;
            c.End = end.TimeOfDay;
            c.Location = location;
            c.Year = (uint)year;
            c.ProfessorId = (uint)int.Parse(instructor.Remove(0, 1));

            var query =
                from course in db.Course
                where course.Number == number
                select course.CourseId;

            foreach (var item in query)
            {
                c.CourseId = (uint)int.Parse(item.ToString());
                break;
            }

            db.Class.Add(c);
            db.SaveChanges();
            if (c.ClassId < 0)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }


        /*******End code to modify********/

    }
}