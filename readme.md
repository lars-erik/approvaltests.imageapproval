## Overview

This is a prototype of a fuzzy image comparison extension to ApprovalTests.

## Disclaimer

This is a 16-20 hour piece of work for now, and have lots of sins.  
It might or might not be polished and stable enough for a nuget release
suddenly during 2020-2021.  
Contributions are wholeheartedly welcomed, tho support and/or code critique is totally unavailable. ;)

There are a bunch of mentions in this doc. that should be linked,
but for now I can't be bothered. They will come, and in the mean time
you can google. Soz., maintainers, but this won't get may views until then
anyway.

## Background

I was originally attempting to use simple binary comparison of screenshots
taken with Selenium to verify webpage design and functionality state
using screenshots.    
A beta site should generally be static enough to be able to produce the same
screenshot locally and on a build server.  
However it turns out fonts like Oswald render completely differently on an
Azure Devops pipeline image than on my computer. 🤷‍♂️  
So I stole some code from ImageDiff and made something that can tell
me how different the images are, and I can control how many CIE76 noticable
differences I allow for a webpage to be approved.  
This is the solution to that problem.

An added bonus is that the resulting code now shows us whatever changed
and/or renders differently for every testrun we do.  
Our designers now have a bulletproof mechanism to discover whether they
messed up a page type they didn't intend to, while fixing another.  
Just like we developers have while developing features.

An added bonus is that screenshots is a wonderful mechanism to smoke test
all the features that we developers fail to do while deploying ourselves. 👼

## Usage

    [UseReporter(typeof(NUnitAttachmentReporter), typeof(NUnitImageComparisonReporter))]
    [TestFixture]
    public class Comparing_Images
    {
        [Test]
        public void Approves_Equal_Images()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks");
            ImageApprovals.Verify(imageBytes, "png");
        }

        [Test]
        public void Approves_Image_With_Tolerance()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks.Nudged");
            ImageApprovals.Verify(imageBytes, "png", 0.021);
        }
    }

When using the NUnitAttachmentReporter, the test will get attachments with
diffs of the received and approved images, as well as statistics for host different
the images are.

The current difference algorithm is a CIE76 color comparison with a
"just noticable" parameter defaulting to 2.3. 

A tolerance for the ratio of pixels that have noticable difference may be set.

## Ideas

