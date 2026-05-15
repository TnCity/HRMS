using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.Entities;

namespace HRMS.DAL.Repositories
{
    public class JobRepository: IJobRepository
    {
        private readonly HRMSDbContext _context;

        public JobRepository(HRMSDbContext context)
        {
            _context = context;
        }
        public List<Job> GetAll()
        {
            return _context.Jobs.ToList();
        }
        public Job GetById(int id)
        {
            return _context.Jobs.Find(id);
        }

        public void Add(Job job)
        {
            _context.Jobs.Add(job);
        }

        public void Update(Job job)
        {
            _context.Jobs.Update(job);
        }

        public void Delete(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job != null)
            {
                _context.Jobs.Remove(job);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

    }
}
