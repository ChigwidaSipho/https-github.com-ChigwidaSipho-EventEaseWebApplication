using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public VenueController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                venue.ImageUrl = await _blobService.UploadFileAsync(imageFile);
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueID) return NotFound();

            var existingVenue = await _context.Venue.FindAsync(id);
            if (existingVenue == null) return NotFound();

            existingVenue.VenueName = venue.VenueName;
            existingVenue.Location = venue.Location;
            existingVenue.Capacity = venue.Capacity;

            if (imageFile != null)
            {
                existingVenue.ImageUrl = await _blobService.UploadFileAsync(imageFile);
            }

            _context.Update(existingVenue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            var hasBookings = await _context.Booking.AnyAsync(b => b.VenueID == id); 
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete venue with bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}