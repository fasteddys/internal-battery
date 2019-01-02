﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
{
    public class EducationalDegreeFactory
    {
        public static EducationalDegree CreateEducationalDegree(string degree)
        {
            EducationalDegree rVal = new EducationalDegree();
            rVal.Degree = degree;
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public EducationalDegree GetOrAdd(UpDiddyDbContext db, string degree)
        {
            degree = degree.Trim();

            EducationalDegree educationalDegree = db.EducationalDegree
                .Where(s => s.IsDeleted == 0 && s.Degree == degree)
                .FirstOrDefault();

            if (educationalDegree == null)
            {
                educationalDegree =  CreateEducationalDegree(degree);
                db.EducationalDegree.Add(educationalDegree);
                db.SaveChanges();
            }
            return educationalDegree;
        }

    }
}
