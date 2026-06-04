using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX + FILTERING
        // =========================
        public async Task<IActionResult> Index(string eventType, DateTime? startDate, DateTime? endDate, bool? isAvailable)
        {
            var events = _context.Event
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(eventType))
                events = events.Where(e => e.EventType != null && e.EventType.TypeName == eventType);

            if (startDate.HasValue)
                events = events.Where(e => e.EventDate >= startDate.Value);

            if (endDate.HasValue)
                events = events.Where(e => e.EventDate <= endDate.Value);

            if (isAvailable.HasValue)
                events = events.Where(e => e.Venue != null && e.Venue.IsAvailable == isAvailable.Value);

            return View(await events.ToListAsync());
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            ModelState.Remove("Venue");
            ModelState.Remove("EventType");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(@event);
                return View(@event);
            }

            _context.Add(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            LoadDropdowns(@event);
            return View(@event);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventID) return NotFound();

            ModelState.Remove("Venue");
            ModelState.Remove("EventType");

            if (!ModelState.IsValid)
            {
                LoadDropdowns(@event);
                return View(@event);
            }

            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Event.Any(e => e.EventID == @event.EventID))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);

            if (@event == null) return NotFound();

            var hasBookings = await _context.Booking.AnyAsync(b => b.EventID == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete event because it has bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPER
        // =========================
        private void LoadDropdowns(Event selectedEvent = null)
        {
            ViewBag.VenueID = new SelectList(
                _context.Venue,
                "VenueID",
                "VenueName", 
                selectedEvent?.VenueID
            ); 
               
            ViewBag.EventTypeID = new SelectList(
                _context.EventType,
                "EventTypeID",
                "TypeName",   
                selectedEvent?.EventTypeID
            );
        }
    }
}