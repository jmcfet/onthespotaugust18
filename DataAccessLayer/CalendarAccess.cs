using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{

    public class CalendarAccess
    {
        BCSContext db = new BCSContext();
        public CalendarAccess()
        {
        }

        public List<Models.Appointment> GetAppointments(out string error)
        {
            error = string.Empty;
            List<Models.Appointment> modelAppts = new List<Models.Appointment>();
            try
            {
                List<Appointment> dbAppts = db.Appointments.ToList();


                foreach (Appointment appt in dbAppts)
                {
                    Models.Appointment model = new Models.Appointment()
                    {
                        //               ID = appt.ID,
                        Body = appt.Body,
                        EndTime = (DateTime)appt.EndTime,
                        Location = appt.Location,
                        StartTime = (DateTime)appt.StartTime,
                        Subject = appt.Subject

                    };
                    modelAppts.Add(model);
                }
            }
            catch (Exception e)
            {
                error = "Critical Error: Could not open Categories DataBase";
            }

            return modelAppts;
        }
        public void DeleteAppointment(string subject,DateTime date)
        {
            Appointment appt = db.Appointments.Where(a => a.Subject == subject && a.StartTime == date).SingleOrDefault();
            if (appt == null)
            {
                
                throw new Exception(string.Format("record not found for {0}", subject));
            }
            db.Appointments.Remove(appt);
            db.SaveChanges();
        }
        public void SaveAppts(Models.Appointment appt)
        {
            Appointment apptdb = new Appointment()
            {
                Body = appt.Body,
                EndTime = appt.EndTime,
                Location = appt.Location,
                StartTime = appt.StartTime,
                Subject = appt.Subject
            };
            db.Appointments.Add(apptdb);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {

            }

        }
    }
                
}
