using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.Entities;

namespace HRMS.DAL.Repositories
{
    
        public interface IJobRepository
        {
            List<Job> GetAll();

            Job GetById(int id);

            void Add(Job job);

            void Update(Job job);

            void Delete(int id);

            void Save();
        }
    
}
