# Test Images for mPlanet Stock Take

This folder contains test images for demonstrating the jewelry stock take functionality.

## Expected Image Files:

The test data expects these image files:
- `ring_001.jpg` - Ring product image
- `necklace_002.jpg` - Necklace product image  
- `earrings_003.jpg` - Earrings product image
- `bracelet_004.jpg` - Bracelet product image
- `brooch_005.jpg` - Brooch product image
- `ring_006.jpg` - Ring product image
- `necklace_007.jpg` - Necklace product image
- `earrings_008.jpg` - Earrings product image
- `bracelet_009.jpg` - Bracelet product image
- `brooch_010.jpg` - Brooch product image

## How to Add Images:

1. Place your jewelry product images in this folder
2. Name them according to the pattern above
3. Supported formats: `.jpg`, `.png`
4. Recommended size: 300x300 pixels or similar square aspect ratio
5. Images will be automatically included in the build as resources

## For Production:

In a real application, images would typically be:
- Stored in a database as BLOBs
- Stored on a file server with URLs in the database  
- Loaded from external image management systems
- Cached locally for offline operation

The PhotoSourceConverter handles missing images gracefully by showing a "No photo" placeholder.