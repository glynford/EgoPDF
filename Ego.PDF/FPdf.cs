﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using MiscUtil.IO;
using MiscUtil.Conversion;

using Ego.PDF.Font;
using Ego.PDF.Data;



/*******************************************************************************
* FPDF                                                                         *
*                                                                              *
* Version: 1.7                                                                 *
* Date:    2011-06-18                                                          *
* Author:  Olivier PLATHEY                                                     *
*******************************************************************************/

/*******************************************************************************
* FPDF.net                                                                     *
* .NET port and adaptation                                                     *
* Version: 1.0 prealpha                                                        *
* Date:    2012-09-30                                                          *
* Author:  Marco Antonio Regueira                                              *
*******************************************************************************/

namespace Ego.PDF
{
    public class FPdf
    {
        public static readonly Encoding PrivateEnconding = Encoding.GetEncoding(1252);

        public readonly string FPDF_VERSION = "1.7";
        /// <summary>
        ///current page number
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// current object number
        /// </summary>
        public int n { get; set; }
        /// <summary>
        /// array of object offsets
        /// </summary>
        public PHP.OrderedMap Offsets { get; set; }
        /// <summary>
        /// buffer holding in-memory PDF
        /// </summary>
        public string Buffer { get; set; }

        /// <summary>
        /// array containing pages
        /// </summary>
        public Dictionary<int, Page> Pages { get; set; }

        /// <summary>
        /// current document state
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// compression flag
        /// </summary>
        public bool Compress { get; set; }
        /// <summary>
        /// scale factor (number of points in user unit)
        /// </summary>
        public double k { get; set; }
        /// <summary>
        /// default orientation
        /// </summary>
        public PageOrientation DefOrientation { get; set; }
        /// <summary>
        /// current orientation
        /// </summary>
        public PageOrientation CurOrientation { get; set; }
        /// <summary>
        /// standard page sizes
        /// </summary>
        public List<PageSize> StdPageSizes { get; set; }
        /// <summary>
        /// default page size
        /// </summary>
        public Dimensions DefPageSize { get; set; }
        /// <summary>
        /// current page size
        /// </summary>
        public Dimensions CurPageSize { get; set; }
        /// <summary>
        /// used for pages with non default sizes or orientations
        /// </summary>
        public Dictionary<int, Dimensions> PageSizes { get; set; }
        /// <summary>
        /// Current page width in points
        /// </summary>
        public double wPt { get; set; }
        /// <summary>
        /// Current page height in points
        /// </summary>
        public double hPt { get; set; }
        /// <summary>
        /// dimensions of current page in user unit
        /// </summary>
        public double w { get; set; }
        public double h { get; set; }
        /// <summary>
        /// left margin
        /// </summary>
        public double LeftMargin { get; set; }
        /// <summary>
        /// top margin
        /// </summary>
        public double TopMargin { get; set; }
        /// <summary>
        /// right margin
        /// </summary>
        public double RightMargin { get; set; }
        /// <summary>
        /// page break margin
        /// </summary>
        public double PageBreakMargin { get; set; }
        public double cMargin { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double lasth { get; set; }
        public double LineWidth { get; set; }
        public List<string> CoreFonts { get; set; }
        Dictionary<string, FontDefinition> Fonts { get; set; }
        Dictionary<string, FontDefinition> FontFiles { get; set; }
        
        public PHP.OrderedMap diffs { get; set; }
        public string FontFamily { get; set; }
        public string FontStyle { get; set; }
        public bool Underline { get; set; }
        public FontDefinition CurrentFont { get; set; }
        public double FontSizePt { get; set; }
        public double FontSize { get; set; }
        public string DrawColor { get; set; }
        public string FillColor { get; set; }
        public string TextColor { get; set; }
        public bool ColorFlag;
        public double ws;
        public Dictionary<string, ImageInfo> images { get; set; }
        public List<LinkDataInternal> links { get; set; }
        public bool AutoPageBreak { get; set; }
        public double PageBreakTrigger { get; set; }
        public bool InHeader { get; set; }
        public bool InFooter { get; set; }
        public string ZoomMode;
        public string LayoutMode;
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Author { get; set; }
        public string Keywords { get; set; }
        public string Creator { get; set; }
        public string AliasNbPages_Renamed { get; set; }
        public string PDFVersion { get; set; }

        public string FPDF_FONTPATH { get; set; }
        private string fontpath { get; set; }


        public FPdf(PageOrientation orientation, UnitEnum unit, PageSizeEnum pageSize)
        {
            pageSize = pageSize == PageSizeEnum.Default ? PageSizeEnum.A4 : pageSize;
            unit = unit == UnitEnum.Default ? UnitEnum.Milimeter : unit;
            pageSize = pageSize== PageSizeEnum.Default? PageSizeEnum.A4: pageSize;

            // Some checks
            double margin;
            this._dochecks();
            // Initialization of properties
            this.Page = 0;
            this.n = 2;
            this.Buffer = PrivateEnconding.GetString(new byte[] { });
            this.Offsets = new PHP.OrderedMap();
            this.Pages = new Dictionary<int,Page>();
            this.PageSizes = new Dictionary<int, Dimensions>();
            this.State = 0;
            this.FontFiles = new Dictionary<string, FontDefinition>();
            this.Fonts = new Dictionary<string, FontDefinition>();
            this.diffs = new PHP.OrderedMap();
            this.images = new Dictionary<string, ImageInfo>();
            this.links = new List<LinkDataInternal>();
            this.InHeader = false;
            this.InFooter = false;
            this.lasth = 0;
            this.FontFamily = "";
            this.FontStyle = "";
            this.FontSizePt = 12;
            this.Underline = false;
            this.DrawColor = "0 G";
            this.FillColor = "0 g";
            this.TextColor = "0 g";
            this.ColorFlag = false;
            this.ws = 0;
            // Font path
            //TODO: SET A DEFAULT FONT PATH
            this.FPDF_FONTPATH = "C:/";
            this.fontpath = FPDF_FONTPATH;

            // Core fonts
            this.CoreFonts = new List<string> { "courier", "helvetica", "times", "symbol", "zapfdingbats" };
            // Scale factor
            this.k = 72 / 25.4; // unidades en milímetros

            // Page sizes
            this.StdPageSizes = new List<PageSize>()
            {
                new PageSize ("a3",841.89, 1190.55),
                new PageSize ("a4",595.28, 841.89),
                new PageSize ("a5",420.94, 595.28),
                new PageSize ("legal",612, 792),
                new PageSize ("letter",612, 1008),
            };

            if (unit == UnitEnum.Point)
            {
                this.k = 1;
            }
            else if (unit == UnitEnum.Milimeter)
            {
                this.k = 72 / 25.4;
            }
            else if (unit == UnitEnum.Centimeter)
            {
                this.k = 72 / 2.54;
            }
            else if (unit == UnitEnum.Inch)
            {
                this.k = 72;
            }
            else
            {
                this.Error("Incorrect unit: " + unit);
            }

            Dimensions size = this._getpagesize(pageSize);
            this.DefPageSize = size;
            this.CurPageSize = size;

            // Page orientation
            this.DefOrientation = orientation;
            this.CurOrientation = this.DefOrientation;

            if (orientation == PageOrientation.Portrait)
            {
                this.DefOrientation = PageOrientation.Portrait;
                this.w = size.Width;
                this.h = size.Heigth;
            }
            else if (orientation == PageOrientation.Landscape)
            {
                this.DefOrientation = PageOrientation.Landscape;
                this.w = size.Heigth;
                this.h = size.Width;
            }
            else
            {
                this.Error("Incorrect orientation: " + orientation);
            }

            this.wPt = this.w * this.k;
            this.hPt = this.h * this.k;
            // Page margins (1 cm)
            margin = 28.35 / this.k;
            this.SetMargins(margin, margin);
            // Interior cell margin (1 mm)
            this.cMargin = margin / 10;
            // Line width (0.2 mm)
            this.LineWidth = .567 / this.k;
            // Automatic page break
            this.SetAutoPageBreak(true, 2 * margin);
            // Default display mode
            this.SetDisplayMode("default", "default");
            // Enable compression

            //TODO: PONER TRUE
            this.SetCompression(false);
            // Set default PDF version number
            this.PDFVersion = "1.3";
        }

        public FPdf():this( PageOrientation.Portrait, UnitEnum.Milimeter, PageSizeEnum.A4)
        {
        }

        /// <summary>
        /// El margen derecho es igual al izquierdo
        /// </name="left"></param>
        /// <param name="top"></param>
        public virtual void SetMargins(double left, double top)
        {
            this.SetMargins(left, top, left);
        }

        /// <summary>
        /// Set left, top and right margins
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        public virtual void SetMargins(double left, double top, double right)
        {
            this.LeftMargin = left;
            this.TopMargin = top;
            this.RightMargin = right;
        }

        public virtual void SetLeftMargin(double margin)
        {
            // Set left margin
            this.LeftMargin = margin;
            if (this.Page > 0 && this.x < margin)
            {
                this.x = margin;
            }
        }

        public virtual void SetTopMargin(double margin)
        {
            // Set top margin
            this.TopMargin = margin;
        }

        public virtual void SetRightMargin(double margin)
        {
            // Set right margin
            this.RightMargin = margin;
        }

        public virtual void SetAutoPageBreak(bool auto, double margin)
        {
            // Set auto page break mode and triggering margin
            this.AutoPageBreak = auto;
            this.PageBreakMargin = margin;
            this.PageBreakTrigger = PHP.TypeSupport.ToDouble(this.h) - margin;
        }

        /// <summary>
        /// Set display mode in viewer
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="layout"></param>
        public virtual void SetDisplayMode(string zoom, string layout)
        {
            if (zoom == "fullpage" || zoom == "fullwidth" || zoom == "real" || zoom == "default" || !(zoom is System.String))
            {
                this.ZoomMode = zoom;
            }
            else
            {
                this.Error("Incorrect zoom display mode: " + zoom);
            }
            if (layout == "single" || layout == "continuous" || layout == "two" || layout == "default")
            {
                this.LayoutMode = layout;
            }
            else
            {
                this.Error("Incorrect layout display mode: " + layout);
            }
        }

        /// <summary>
        /// Set page compression
        /// </summary>
        /// <param name="compress"></param>
        public virtual void SetCompression(bool compress)
        {
            this.Compress = compress;
        }

        /// <summary>
        /// Title of document
        /// </summary>
        /// <param name="title"></param>
        public virtual void SetTitle(string title)
        {
            this.Title = title;
        }

        /// <summary>
        /// Subject of document
        /// </summary>
        /// <param name="subject"></param>
        public virtual void SetSubject(string subject)
        {
            this.Subject = subject;
        }

        /// <summary>
        /// Keywords of document
        /// </summary>
        /// <param name="author"></param>
        public virtual void SetAuthor(string author)
        {
            // Author of document
            this.Author = author;
        }

        /// <summary>
        /// Keywords of document
        /// </summary>
        /// <param name="keywords"></param>
        public virtual void SetKeywords(string keywords)
        {
            this.Keywords = keywords;
        }

        /// <summary>
        /// Creator of document 
        /// </summary>
        /// <param name="creator"></param>
        public virtual void SetCreator(string creator)
        {
            this.Creator = creator;
        }

        /// <summary>
        /// Define an alias for total number of pages
        /// </summary>
        public virtual void AliasNbPages()
        {
            this.AliasNbPages("{nb}");
        }

        /// <summary>
        /// Define an alias for total number of pages
        /// </summary>
        /// <param name="alias"></param>
        public virtual void AliasNbPages(string alias)
        {
            this.AliasNbPages_Renamed = alias;
        }

        /// <summary>
        /// Fatal error
        /// </summary>
        /// <param name="msg"></param>
        public virtual void Error(string msg)
        {
            throw new InvalidOperationException(msg);
        }

        /// <summary>
        /// Begin document
        /// </summary>
        public virtual void Open()
        {
            this.State = 1;
        }

        /// <summary>
        /// Terminate document
        /// </summary>
        public virtual void Close()
        {
            if (this.State == 3)
            {
                return;
            }
            if (this.Page == 0)
            {
                this.AddPage(PageOrientation.Default, null);
            }
            // Page footer
            this.InFooter = true;
            this.Footer();
            this.InFooter = false;
            // Close page
            this._endpage();
            // Close document
            this._enddoc();
        }

        public virtual void AddPage()
        {
            this.AddPage(PageOrientation.Default,this.DefPageSize);
        }

        public virtual void AddPage(PageOrientation orientation)
        {
            AddPage(orientation, PageSizeEnum.Default);
        }

        public virtual void AddPage(PageSizeEnum size)
        {
            AddPage(PageOrientation.Default, size);
        }

        public virtual void AddPage(PageOrientation orientation, PageSizeEnum pagesize)
        {
            Dimensions page = _getpagesize(pagesize);
            this.AddPage(orientation, page);
        }

        public virtual void AddPage(PageOrientation orientation, Dimensions size)
        {
            // Start a new page
            string family;
            string style;
            double fontsize;
            double lw;
            string dc;
            string fc;
            string tc;
            bool cf;
            if (this.State == 0)
            {
                this.Open();
            }
            family = this.FontFamily;
            style = this.FontStyle + (this.Underline ? "U" : "");
            fontsize = this.FontSizePt;
            lw = this.LineWidth;
            dc = this.DrawColor;
            fc = this.FillColor;
            tc = this.TextColor;
            cf = this.ColorFlag;
            if (this.Page > 0)
            {
                // Page footer
                this.InFooter = true;
                this.Footer();
                this.InFooter = false;
                // Close page
                this._endpage();
            }
            // Start new page
            this._beginpage(orientation, size);
            // Set line cap style to square
            this._out("2 J");
            // Set line width
            this.LineWidth = lw;
            this._out(sprintf("%.2F w", lw * this.k));

            // Set font
            if (PHP.TypeSupport.ToBoolean(family))
            {
                this.SetFont(family, style, fontsize);
            }
            // Set colors
            this.DrawColor = dc;
            if (PHP.TypeSupport.ToString(dc) != "0 G")
            {
                this._out(dc);
            }
            this.FillColor = fc;
            if (PHP.TypeSupport.ToString(fc) != "0 g")
            {
                this._out(fc);
            }
            this.TextColor = tc;
            this.ColorFlag = cf;
            // Page header
            this.InHeader = true;
            this.Header();
            this.InHeader = false;
            // Restore line width
            if (this.LineWidth != lw)
            {
                this.LineWidth = lw;
                this._out(sprintf("%.2F w", lw * this.k));
            }
            // Restore font
            if (PHP.TypeSupport.ToBoolean(family))
            {
                this.SetFont(family, style, fontsize);
            }
            // Restore colors
            //CONVERSION_WARNING: Converted Operator might not behave as expected. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1009.htm 
            if (this.DrawColor != dc)
            {
                this.DrawColor = dc;
                this._out(dc);
            }
            //CONVERSION_WARNING: Converted Operator might not behave as expected. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1009.htm 
            if (this.FillColor != fc)
            {
                this.FillColor = fc;
                this._out(fc);
            }
            this.TextColor = tc;
            this.ColorFlag = cf;
        }

        public virtual void Header()
        {
            // To be implemented in your own inherited class
        }

        public virtual void Footer()
        {
            // To be implemented in your own inherited class
        }

        public virtual int PageNo()
        {
            // Get current page number
            return this.Page;
        }

        public virtual void SetDrawColor(int red, int? green, int? blue)
        {
            double r = red;
            double? g = green;
            double? b = blue;

            // Set color for all stroking operations
            if ((r == 0 && g == 0 && b == 0) || (!g.HasValue))
            {
                this.DrawColor = sprintf("%.3F G", r / 255);
            }
            else
            {
                this.DrawColor = sprintf("%.3F %.3F %.3F RG", r / 255, g / 255, b / 255);
            }
            if (this.Page > 0)
            {
                this._out(this.DrawColor);
            }
        }

        public virtual void SetFillColor(int grey)
        {
            this.FillColor = sprintf("%.3F g", grey / 255);
            this.ColorFlag = (this.FillColor != this.TextColor);
            if (this.Page > 0)
            {
                this._out(this.FillColor);
            }
        }

        public virtual void SetFillColor(int red, int green, int blue)
        {
            double r = red;
            double g = green;
            double b = blue;

            // Set color for all filling operations
            this.FillColor = sprintf("%.3F %.3F %.3F rg", r / 255, g / 255, b / 255);
            this.ColorFlag = (this.FillColor != this.TextColor);
            if (this.Page > 0)
            {
                this._out(this.FillColor);
            }
        }

        public void SetTextColor(int greyColor)
        {
            this.TextColor = sprintf("%.3F g", greyColor / 255);
            this.ColorFlag = (this.FillColor != this.TextColor);
        }

        public virtual void SetTextColor(int red, int green, int blue)
        {
            double r = red;
            double g = green;
            double b = blue;

            this.TextColor = sprintf("%.3F %.3F %.3F rg", r / 255, g / 255, b / 255);
            this.ColorFlag = (this.FillColor != this.TextColor);
        }

        public virtual double GetStringWidth(string s)
        {
            double w;
            int l;
            int i;

            w = 0;
            l = PHP.TypeSupport.ToString(s).Length;

            for (i = 0; i < l; i++)
                w = w + PHP.TypeSupport.ToDouble(this.CurrentFont.Widths[s[i].ToString()]);

           
            return w * this.FontSize / 1000;
        }

        public virtual void SetLineWidth(double width)
        {
            // Set line width
            this.LineWidth = width;
            if (this.Page > 0)
            {
                this._out(sprintf("%.2F w", width * this.k));
            }
        }

        public virtual void Line(double x1, double y1, double x2, double y2)
        {
            // Draw a line
            this._out(sprintf("%.2F %.2F m %.2F %.2F l S",
                x1 * this.k,
                (this.h - y1) * this.k, x2 * this.k,
                (this.h - y2) * this.k));
        }

        public virtual void Rect(double x, double y, double w, double h, string style)
        {
            // Draw a rectangle
            string op;
            if (style == "F")
            {
                op = "f";
            }
            else if (style == "FD" || style == "DF")
            {
                op = "B";
            }
            else
            {
                op = "S";
            }
            this._out(sprintf("%.2F %.2F %.2F %.2F re %s", x * this.k, (this.h - y) * this.k, w * this.k, (-h) * this.k, op));
        }

        public virtual void AddFont(string family, string style, string file)
        {
            // Add a TrueType, OpenType or Type1 font
            string fontkey;
            int n;
            family = family.ToLower();
            if (file == "")
            {
                //CONVERSION_WARNING: Method 'str_replace' was converted to 'PHP.StringSupport.StringReplace' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/str_replace.htm 
                file = PHP.TypeSupport.ToString(PHP.StringSupport.StringReplace(family, " ", "")) + style.ToLower();
            }
            style = style.ToUpper();
            if (style == "IB")
            {
                style = "BI";
            }

            fontkey = family + style;
            if (this.Fonts.ContainsKey(fontkey))
            {
                return;
            }

            FontDefinition fontInfo = this._loadfont(file);
            fontInfo.i = PHP.OrderedMap.CountElements(this.Fonts) + 1;

            if (fontInfo.diff != null)
            {
                // Search existing encodings
                n = Convert.ToInt32(this.diffs.Search(fontInfo.diff));
                if (!System.Convert.ToBoolean(n))
                {
                    n = PHP.OrderedMap.CountElements(this.diffs) + 1;
                    this.diffs[n] = fontInfo.diff;
                }
                fontInfo.diffn = n;
            }

            if (!string.IsNullOrEmpty(fontInfo.file))
            {
                // Embedded font
                if (fontInfo.type == FontTypeEnum.TrueType)
                {
                    this.FontFiles[fontInfo.file] = new FontDefinition()
                    {
                        length1 = fontInfo.originalsize
                    };
                }
                else
                {
                    this.FontFiles[fontInfo.file] = new FontDefinition()
                    {
                        length1 = fontInfo.size1,
                        length2 = fontInfo.size2
                    };
                }
            }
            this.Fonts[fontkey] = fontInfo;
        }


        public virtual void SetFont(string family)
        {
            SetFont(family, string.Empty);
        }

        public virtual void SetFont(string family, string style)
        {
            SetFont(family, style, 0);
        }

        public virtual void SetFont(string family, string style, double size)
        {
            // Select a font; size given in points
            string fontkey;
            if (string.IsNullOrEmpty(family))
            {
                family = this.FontFamily;
            }
            else
            {
                family = family.ToLower();
            }
            style = (style ?? string.Empty).ToUpper();
            if (style.Contains("U"))
            {
                this.Underline = true;
                //CONVERSION_WARNING: Method 'str_replace' was converted to 'PHP.StringSupport.StringReplace' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/str_replace.htm 
                style = PHP.TypeSupport.ToString(PHP.StringSupport.StringReplace(style, "U", ""));
            }
            else
            {
                this.Underline = false;
            }
            if (style == "IB")
            {
                style = "BI";
            }
            if (size == 0)
            {
                size = this.FontSizePt;
            }
            // Test if font is already selected
            if (this.FontFamily == family && this.FontStyle == style && this.FontSizePt == size)
            {
                return;
            }
            // Test if font is already loaded
            fontkey = family + style;
            if (!this.Fonts.ContainsKey(fontkey))
            {
                // Test if one of the core fonts
                if (family == "arial")
                {
                    family = "helvetica";
                }
                if (this.CoreFonts.Contains(family))
                {
                    if (family == "symbol" || family == "zapfdingbats")
                    {
                        style = "";
                    }
                    fontkey = family + style;
                    if (!(this.Fonts.ContainsKey(fontkey)))
                    {
                        this.AddFont(family, style, "");
                    }
                }
                else
                {
                    this.Error("Undefined font: " + family + " " + style);
                }
            }
            // Select it
            this.FontFamily = family;
            this.FontStyle = style;
            this.FontSizePt = size;
            this.FontSize = size / this.k;
            this.CurrentFont = this.Fonts[fontkey];
            if (this.Page > 0)
            {
                this._out(sprintf("BT /F%d %.2F Tf ET", this.CurrentFont.i, this.FontSizePt));
            }
        }

        public virtual void SetFontSize(double size)
        {
            // Set font size in points
            if (this.FontSizePt == size)
            {
                return;
            }
            this.FontSizePt = size;
            this.FontSize = size / this.k;
            if (this.Page > 0)
            {
                this._out(sprintf("BT /F%d %.2F Tf ET", this.CurrentFont.i, this.FontSizePt));
            }
        }

        public LinkDataInternal AddLink()
        {
            // Create a new internal link
            int n;
            LinkDataInternal l = new LinkDataInternal();
            this.links.Add(l);
            return l;
        }


        public void SetLink(LinkDataInternal link)
        {
            link.PageIndex= this.Page;
            link.Y = this.y;
        }

        public LinkDataInternal SetLink(int link)
        {
            var l =this.SetLink(link, 0, -1);
            return l;
        }

        public virtual LinkDataInternal SetLink(int link, double y)
        {
            var l = this.SetLink(link, y, -1);
            return l;
        }

        public virtual LinkDataInternal SetLink(int link, double y, int page)
        {
            // Set destination of internal link
            if (y == -1)
            {
                y = (int)(this.y);
            }
            if (page == -1)
            {
                page = this.Page;
            }
            LinkDataInternal linkInternal=new LinkDataInternal(page, y);
            this.links[link] = linkInternal;
            return linkInternal;
        }

        public virtual void Link(double x, double y, double w, double h, LinkData link)
        {
            this.Pages[this.Page].PageLinks.Add(new PageLink(x * this.k, this.hPt - y * this.k, w * this.k, h * this.k, link));
        }

        public virtual void Text(double x, double y, string txt)
        {
            // Output a string
            object s;
            s = sprintf("BT %.2F %.2F Td (%s) Tj ET", x * this.k, (this.h - y) * this.k, this._escape(txt));
            if (this.Underline && PHP.TypeSupport.ToString(txt) != "")
            {
                s = PHP.TypeSupport.ToString(s) + " " + this._dounderline(x, y, txt);
            }
            if (this.ColorFlag)
            {
                s = "q " + PHP.TypeSupport.ToString(this.TextColor) + " " + PHP.TypeSupport.ToString(s) + " Q";
            }
            this._out(s);
        }

        public virtual bool AcceptPageBreak()
        {
            // Accept automatic page break or not
            return this.AutoPageBreak;
        }

        public virtual void Cell(double w)
        {
            this.Cell(w, null, null, "0", 0, AlignEnum.Default, false, null);
        }

        public virtual void Cell(double w, double? h, string txt)
        {
            this.Cell(w, h, txt, "0", 0, AlignEnum.Default, false, null);
        }

        public virtual void Cell(double w, double? h, string txt, string border)
        {
            this.Cell(w, h, txt, border, 0, AlignEnum.Default, false, null);
        }

        public virtual void Cell(double w, double? h, string txt, string border, double ln)
        {
            this.Cell(w, h, txt, border, ln , AlignEnum.Default, false, null);
        }

        public virtual void Cell(double w, double? h, string txt, string border, double ln, AlignEnum align)
        {
            this.Cell(w, h, txt, border, ln, align, false, null);
        }

        public virtual void Cell(double w, double? h, string txt, string border, double ln, AlignEnum align, bool fill)
        {
            this.Cell(w, h, txt, border, ln, align, fill, null);
        }

        public virtual void Cell(double w, double? h, string txt, string border, double ln, AlignEnum align, bool fill, LinkData link)
        {
            // Output a cell
            double k;
            double x;
            double ws;
            string s;
            string op;
            double y;
            double dx;
            object txt2;
            k = this.k;

            if (!h.HasValue) h = 0;

            if (this.y + h > this.PageBreakTrigger && !this.InHeader && !this.InFooter && this.AcceptPageBreak())
            {
                // Automatic page break
                x = this.x;
                ws = this.ws;
                if (ws > 0)
                {
                    this.ws = 0;
                    this._out("0 Tw");
                }
                this.AddPage(this.CurOrientation, this.CurPageSize);
                this.x = x;
                if (ws > 0)
                {
                    this.ws = ws;

                    this._out(sprintf("%.3F Tw", ws * k));
                }
            }
            if (w == 0)
            {
                w = PHP.TypeSupport.ToDouble(this.w) - this.RightMargin - this.x;
            }
            s = "";
            if (System.Convert.ToBoolean(fill) || PHP.TypeSupport.ToInt32(border) == 1)
            {
                if (fill)                {
                    op = (PHP.TypeSupport.ToInt32(border) == 1) ? "B" : "f";
                }
                else
                {
                    op = "S";
                }
                s = sprintf("%.2F %.2F %.2F %.2F re %s ", this.x * k, (this.h - this.y) * k, w * k, -h * k, op);
            }
            if (!string.IsNullOrEmpty(border))
            {
                x = this.x;
                y = this.y;
                if (border.Contains("L"))
                {
                    s = PHP.TypeSupport.ToString(s) + sprintf("%.2F %.2F m %.2F %.2F l S ", x * k, (this.h - y) * k, x * k, (this.h - (y + h)) * k);
                }
                if (border.Contains("T"))
                {
                    s = PHP.TypeSupport.ToString(s) + sprintf("%.2F %.2F m %.2F %.2F l S ", x * k, (this.h - y) * k, (x + w) * k, (this.h - y) * k);
                }
                if (border.Contains("R"))
                {
                    s = PHP.TypeSupport.ToString(s) + sprintf("%.2F %.2F m %.2F %.2F l S ", (x + w) * k, (this.h - y) * k, (x + w) * k, (this.h - (y + h)) * k);
                }
                if (border.Contains("B"))
                {
                    s = PHP.TypeSupport.ToString(s) + sprintf("%.2F %.2F m %.2F %.2F l S ", x * k, (this.h - (y + h)) * k, (x + w) * k, (this.h - (y + h)) * k);
                }
            }
            if (!string.IsNullOrEmpty(txt))
            {
                if (align == AlignEnum.Right)
                {
                    dx = w - this.cMargin - this.GetStringWidth(txt);
                }
                else if (align == AlignEnum.Center)
                {
                    dx = (w - this.GetStringWidth(txt)) / 2;
                }
                else
                {
                    dx = this.cMargin;
                }
                if (this.ColorFlag)
                {
                    s = PHP.TypeSupport.ToString(s) + "q " + PHP.TypeSupport.ToString(this.TextColor) + " ";
                }

                txt2 = txt
                    .Replace("\\", "\\\\")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)");
                
                s = PHP.TypeSupport.ToString(s) + sprintf("BT %.2F %.2F Td (%s) Tj ET", (this.x + dx) * k, (this.h - (this.y + .5 * h + .3 * this.FontSize)) * k, txt2);
                if (this.Underline)
                {
                    s = PHP.TypeSupport.ToString(s) + " " + this._dounderline(this.x + dx, this.y + .5 * h.Value + .3 * this.FontSize, txt);
                }
                if (this.ColorFlag)
                {
                    s = PHP.TypeSupport.ToString(s) + " Q";
                }
                if (PHP.TypeSupport.ToBoolean(link))
                {
                    this.Link(this.x + dx, this.y + .5 * h.Value - .5 * this.FontSize, this.GetStringWidth(txt), this.FontSize, link);
                }
            }
            if (!string.IsNullOrEmpty(s))
            {
                this._out(s);
            }
            this.lasth = h.Value;
            if (ln > 0)
            {
                // Go to next line
                this.y += h.Value;
                if (ln == 1)
                {
                    this.x = this.LeftMargin;
                }
            }
            else
            {
                this.x += w;
            }
        }

        public virtual void MultiCell(double w, int h, string txt)
        {
            MultiCell(w, h, txt, null, AlignEnum.Default, false);
        }

        public virtual void MultiCell(double w, int h, string txt, string border, AlignEnum align, bool fill)
        {
            if (align == AlignEnum.Default)
            {
                align = AlignEnum.Justified;
            }

            // Output text with automatic or explicit line breaks
            double wmax;
            string s;
            int nb;
            string b;
            string b2 = string.Empty;
            int sep;
            int i;
            int j;
            double l;
            int ns;
            int nl;
            string c;
            double ls = 0;
            FontDefinition cw = this.CurrentFont;
            if (w == 0)
            {
                w = PHP.TypeSupport.ToDouble(this.w) - this.RightMargin - this.x;
            }
            wmax = (w - 2 * this.cMargin) * 1000 / this.FontSize;
            //CONVERSION_WARNING: Method 'str_replace' was converted to 'PHP.StringSupport.StringReplace' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/str_replace.htm 
            s = PHP.StringSupport.StringReplace(txt, "\r", "");
            nb = PHP.TypeSupport.ToString(s).Length;
            if (nb > 0 && PHP.TypeSupport.ToString(s[nb - 1]) == "\n")
            {
                nb--;
            }
            b = 0.ToString();
            if (PHP.TypeSupport.ToBoolean(border))
            {
                if (PHP.TypeSupport.ToInt32(border) == 1)
                {
                    border = "LTRB";
                    b = "LRT";
                    b2 = "LR";
                }
                else
                {
                    b2 = "";
                    //CONVERSION_TODO: The equivalent in .NET for strpos may return a different value. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1007.htm 
                    if (!(border.IndexOf("L") == System.Convert.ToInt32(false)) || !(border.IndexOf("L").GetType() == false.GetType()))
                    {
                        b2 += "L";
                    }
                    //CONVERSION_TODO: The equivalent in .NET for strpos may return a different value. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1007.htm 
                    if (!(border.IndexOf("R") == System.Convert.ToInt32(false)) || !(border.IndexOf("R").GetType() == false.GetType()))
                    {
                        b2 += "R";
                    }
                    //CONVERSION_TODO: The equivalent in .NET for strpos may return a different value. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1007.htm 
                    b = (!(border.IndexOf("T") == System.Convert.ToInt32(false)) || !(border.IndexOf("T").GetType() == false.GetType())) ? b2 + "T" : b2;
                }
            }
            sep = -1;
            i = 0;
            j = 0;
            l = 0;
            ns = 0;
            nl = 1;
            while (i < nb)
            {
                // Get next character
                c = s[i].ToString();
                if (PHP.TypeSupport.ToString(c) == "\n")
                {
                    // Explicit line break
                    if (this.ws > 0)
                    {
                        this.ws = 0;
                        this._out("0 Tw");
                    }
                    //CONVERSION_WARNING: Method 'substr' was converted to 'System.String.Substring' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/substr.htm 
                    this.Cell(w, h, PHP.TypeSupport.ToString(s).Substring(j, i - j), b, 2, align, fill,null);
                    i++;
                    sep = -1;
                    j = i;
                    l = 0;
                    ns = 0;
                    nl++;
                    if (PHP.TypeSupport.ToBoolean(border) && nl == 2)
                    {
                        b = b2;
                    }
                    continue;
                }
                if (PHP.TypeSupport.ToString(c) == " ")
                {
                    sep = i;
                    ls = l;
                    ns++;
                }
                l = l + PHP.TypeSupport.ToDouble(cw.Widths[c]);
                if (l > wmax)
                {
                    // Automatic line break
                    if (sep == -1)
                    {
                        if (i == j)
                        {
                            i++;
                        }
                        if (this.ws > 0)
                        {
                            this.ws = 0;
                            this._out("0 Tw");
                        }
                        //CONVERSION_WARNING: Method 'substr' was converted to 'System.String.Substring' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/substr.htm 
                        this.Cell(w, h, PHP.TypeSupport.ToString(s).Substring(j, i - j), b, 2, align, fill, null);
                    }
                    else
                    {
                        if (align == AlignEnum.Justified)
                        {
                            this.ws = (ns > 1) ? (wmax - ls) / 1000 * this.FontSize / (ns - 1) : 0;

                            this._out(sprintf("%.3F Tw", this.ws * this.k));
                        }
                        //CONVERSION_WARNING: Method 'substr' was converted to 'System.String.Substring' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/substr.htm 
                        this.Cell(w, h, PHP.TypeSupport.ToString(s).Substring(j, sep - j), b, 2, align, fill, null);
                        i = sep + 1;
                    }
                    sep = -1;
                    j = i;
                    l = 0;
                    ns = 0;
                    nl++;
                    if (PHP.TypeSupport.ToBoolean(border) && nl == 2)
                    {
                        b = b2;
                    }
                }
                else
                {
                    i++;
                }
            }
            // Last chunk
            if (this.ws > 0)
            {
                this.ws = 0;
                this._out("0 Tw");
            }
            //CONVERSION_TODO: The equivalent in .NET for strpos may return a different value. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1007.htm 
            if (PHP.TypeSupport.ToBoolean(border) && (!(border.IndexOf("B") == System.Convert.ToInt32(false)) || !(border.IndexOf("B").GetType() == false.GetType())))
            {
                b += "B";
            }
            this.Cell(w, h, PHP.TypeSupport.ToString(s).Substring(j, i - j), b, 2, align, fill, null);
            this.x = this.LeftMargin;
        }

        public virtual void Write(int h, string txt)
        {
            this.Write(h, txt, (LinkData)null);
        }

        public virtual void Write(int h, string txt, int internalLink)
        {
            var link = this.links[internalLink];
            this.Write(h, txt, internalLink);
        }

        public virtual void Write(int h, string txt, string uri)
        {
            LinkDataUri data = new LinkDataUri( uri);
            this.Write(h, txt, uri);
        }

        protected virtual void Write(int h, string txt, LinkData link)
        {
            // Output text in flowing mode
            double w;
            double wmax;
            string s;
            double nb;
            int sep;
            int i;
            int j;
            int l;
            int nl;
            string c;
            FontDefinition cw = this.CurrentFont;
            w = this.w - this.RightMargin - this.x;
            wmax = (w - 2 * this.cMargin) * 1000 / this.FontSize;
            s = PHP.StringSupport.StringReplace(txt, "\r", "");
            nb = PHP.TypeSupport.ToString(s).Length;
            sep = -1;
            i = 0;
            j = 0;
            l = 0;
            nl = 1;
            while (i < nb)
            {
                // Get next character
                c = s[i].ToString();
                if (PHP.TypeSupport.ToString(c) == "\n")
                {
                    // Explicit line break
                    this.Cell(w, h, s.Substring(j, i - j), 0.ToString(), 2, AlignEnum.Default, false, link);
                    i++;
                    sep = -1;
                    j = i;
                    l = 0;
                    if (nl == 1)
                    {
                        this.x = this.LeftMargin;
                        w = this.w - this.RightMargin - this.x;
                        wmax = (w - 2 * this.cMargin) * 1000 / this.FontSize;
                    }
                    nl++;
                    continue;
                }
                if (PHP.TypeSupport.ToString(c) == " ")
                {
                    sep = i;
                }
                l = l + cw.Widths[c];
                if (l > wmax)
                {
                    // Automatic line break
                    if (sep == -1)
                    {
                        if (this.x > this.LeftMargin)
                        {
                            // Move to next line
                            this.x = this.LeftMargin;
                            this.y += h;
                            w = this.w - this.RightMargin - this.x;
                            wmax = (w - 2 * this.cMargin) * 1000 / this.FontSize;
                            i++;
                            nl++;
                            continue;
                        }
                        if (i == j)
                        {
                            i++;
                        }
                        this.Cell(w, h, s.Substring(j, i - j), 0.ToString(), 2, AlignEnum.Default, false, link);
                    }
                    else
                    {
                        this.Cell(w, h, s.Substring(j, sep - j), 0.ToString(), 2, AlignEnum.Default, false, link);
                        i = sep + 1;
                    }
                    sep = -1;
                    j = i;
                    l = 0;
                    if (nl == 1)
                    {
                        this.x = this.LeftMargin;
                        w = this.w - this.RightMargin - this.x;
                        wmax = (w - 2 * this.cMargin) * 1000 / this.FontSize;
                    }
                    nl++;
                }
                else
                {
                    i++;
                }
            }
            // Last chunk
            if (i != j)
            {
                this._out(l + " " + this.x + " " + this.ws + " " + this.RightMargin);
                this.Cell(l / 1000 * this.FontSize, h, s.Substring(j), 0.ToString(), 0, AlignEnum.Default, false, link);
                this._out(l + " " + this.x + " " + this.ws + " " + this.RightMargin);
            }
        }

        public virtual void Ln()
        {
            // Line feed; default value is last cell height
            this.x = this.LeftMargin;
            this.y += this.lasth;
        }

        public virtual void Ln(int h)
        {
            // Line feed; 
            this.x = this.LeftMargin;
            this.y += h;
        }

        public virtual void Image(string file, double? x, double? y, double w, double h)
        {
            Image(file, x, y, w, h, ImageTypeEnum.Default, (LinkData)null);
        }

        public virtual void Image(string file, double w, double h, ImageTypeEnum type, LinkData link)
        {
            Image(file, null, null, w, h, type, link);
        }

        public virtual void Image(string file, double w, double h, ImageTypeEnum type, string link)
        {
            Image(file, null, null, w, h, type, new LinkDataUri(link));
        }

        public virtual void Image(string file, double? x, double? y, double w, double h, ImageTypeEnum type, string link)
        {
            this.Image(file, x, y, w, h, type, new LinkDataUri(link));
        }

        public virtual void Image(string file, double? x, double? y, double w, double h, ImageTypeEnum type, LinkData link)
        {
            // Put an image on the page
            ImageInfo imageInfo = new ImageInfo();
            double x2;

            if (!this.images.ContainsKey(file))
            {
                // First use of this image, get info
                if (type == ImageTypeEnum.Default)
                {
                    if (!Enum.TryParse<ImageTypeEnum>(Path.GetExtension(file).Replace(".", string.Empty), true, out type))
                    {
                        this.Error("Image file has no extension and no type was specified: " + file);
                    }
                }

                ImageInfo imageData = null;
                switch (type)
                {
                    case ImageTypeEnum.Jpg:
                        imageData = _parsejpg(file);
                        break;
                    case ImageTypeEnum.Png:
                        imageData = _parsepng(file);
                        break;
                    case ImageTypeEnum.Gif:
                        imageData = _parsegif(file);
                        break;
                    case ImageTypeEnum.Default:
                    default:
                        this.Error("Image file has no extension and no type was specified or unsupported type (" + file + ")");
                        break;
                }
                String typeName = type.ToString().ToLower();
                //CONVERSION_ISSUE: Variable function '$mtd' was not converted. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1000.htm 
                imageInfo = imageData;
                imageInfo.i = PHP.OrderedMap.CountElements(this.images) + 1;
                this.images[file] = imageInfo;
            }
            else
            {
                imageInfo = this.images[file];
            }

            // Automatic width and height calculation if needed
            if (w == 0 && h == 0)
            {
                // Put image at 96 dpi
                w = -96;
                h = -96;
            }
            if (w < 0)
            {
                w = PHP.TypeSupport.ToDouble(-PHP.TypeSupport.ToDouble(imageInfo.w)) * 72 / w / this.k;
            }
            if (h < 0)
            {
                h = PHP.TypeSupport.ToDouble(-PHP.TypeSupport.ToDouble(imageInfo.h)) * 72 / h / this.k;
            }
            if (w == 0)
            {
                w = h * PHP.TypeSupport.ToDouble(imageInfo.w) / PHP.TypeSupport.ToDouble(imageInfo.h);
            }
            if (h == 0)
            {
                h = w * PHP.TypeSupport.ToDouble(imageInfo.h) / PHP.TypeSupport.ToDouble(imageInfo.w);
            }

            // Flowing mode
            if (!y.HasValue)
            {
                if (this.y + h > this.PageBreakTrigger && !this.InHeader && !this.InFooter && this.AcceptPageBreak())
                {
                    // Automatic page break
                    x2 = this.x;
                    this.AddPage(this.CurOrientation, this.CurPageSize);
                    this.x = x2;
                }
                y = this.y;
                this.y += h;
            }

            if (!x.HasValue)
            {
                x = this.x;
            }
            this._out(sprintf("q %.2F 0 0 %.2F %.2F %.2F cm /I%d Do Q", w * this.k, h * this.k, x * this.k, ((this.h - (y + h)) * this.k), imageInfo.i));
            if (link!=null)
            {
                this.Link(x.Value, y.Value, w, h, link);
            }
        }

        public virtual double GetX()
        {
            // Get x position
            return this.x;
        }

        public virtual void SetX(double x)
        {
            // Set x position
            if (x >= 0)
            {
                this.x = x;
            }
            else
            {
                this.x = PHP.TypeSupport.ToDouble(this.w) + x;
            }
        }

        public virtual double GetY()
        {
            // Get y position
            return this.y;
        }

        public virtual void SetY(double y)
        {
            // Set y position and reset x
            this.x = this.LeftMargin;
            if (y >= 0)
            {
                this.y = y;
            }
            else
            {
                this.y = PHP.TypeSupport.ToDouble(this.h) + y;
            }
        }

        public virtual void SetXY(double x, double y)
        {
            // Set x and y positions
            this.SetY(y);
            this.SetX(x);
        }

        public virtual string Output(string name, OutputDevice dest)
        {
            // Output PDF to some destination
            System.IO.FileStream f;
            if (this.State < 3)
            {
                this.Close();
            }
            if (dest == OutputDevice.Default)
            {
                if (name == "" || name == null)
                {
                    name = "doc.pdf";
                    dest = OutputDevice.StandardOutput;
                }
                else
                {
                    dest = OutputDevice.SaveToFile;
                }
            }
            switch (dest)
            {
                case OutputDevice.StandardOutput:
                    this._checkoutput();
                    HttpContext.Current.Response.AppendHeader("Content-Type: application/pdf", "");
                    HttpContext.Current.Response.AppendHeader("Content-Disposition: inline; filename=\"" + name + "\"", "");
                    HttpContext.Current.Response.AppendHeader("Cache-Control: private, max-age=0, must-revalidate", "");
                    HttpContext.Current.Response.AppendHeader("Pragma: public", "");
                    HttpContext.Current.Response.Write(this.Buffer);
                    break;

                case OutputDevice.Download:
                    // Download file
                    this._checkoutput();
                    HttpContext.Current.Response.AppendHeader("Content-Type: application/x-download", "");
                    HttpContext.Current.Response.AppendHeader("Content-Disposition: attachment; filename=\"" + name + "\"", "");
                    HttpContext.Current.Response.AppendHeader("Cache-Control: private, max-age=0, must-revalidate", "");
                    HttpContext.Current.Response.AppendHeader("Pragma: public", "");
                    HttpContext.Current.Response.Write(this.Buffer);
                    break;

                case OutputDevice.SaveToFile:
                    // Save to local file
                    f = PHP.FileSystemSupport.FileOpen(name, "wb");
                    if (!PHP.TypeSupport.ToBoolean(f))
                    {
                        this.Error("Unable to create output file: " + name);
                    }
                    StreamWriter w=new StreamWriter(f,PrivateEnconding);
                    w.Write(this.Buffer);
                    w.Close();
                    //PHP.FileSystemSupport.Write(f, this.Buffer, this.Buffer.Length);
                    //CONVERSION_WARNING: Method 'fclose' was converted to 'PHP.FileSystemSupport.Close' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/fclose.htm 
                    //PHP.FileSystemSupport.Close(f);
                    break;

                case OutputDevice.ReturnAsString:
                    // Return as a string
                    return this.Buffer;

                default:
                    //CONVERSION_ISSUE: Void functions cannot be used as part of an expression. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1019.htm 
                    this.Error("Incorrect output destination: " + dest);
                    break;

            }
            return "";
        }

        /*******************************************************************************
        *                                                                              *
        *                              Protected methods                               *
        *                                                                              *
        *******************************************************************************/
        internal virtual void _dochecks()
        {
            // Check mbstring overloading --> mbstring overloading must be disabled
            // Ensure runtime magic quotes are disabled set_magic_quotes_runtime(0);
        }

        internal virtual void _checkoutput()
        {

            //TODO COMPROBAR ESTO
            var HeadersOrDataSent = false;

            if (HeadersOrDataSent)
            {
                {
                    this.Error("Some data has already been output, can't send PDF file");
                }
            }
        }

        internal virtual Dimensions _getpagesize(PageSizeEnum index)
        {
            PageSize size = this.StdPageSizes.FirstOrDefault(x => x.Name.ToLower() == index.ToString().ToLower());
            return size.GetDimensions(this.k);
        }

        internal virtual Dimensions _getpagesize(Dimensions dimensions)
        {
            if (dimensions.Width > dimensions.Heigth)
            {
                return new Dimensions() { Width = dimensions.Heigth, Heigth = dimensions.Width };
            }
            else return dimensions;
        }

        internal virtual void _beginpage(PageOrientation orientation, Dimensions size)
        {
            this.Page++;
            this.Pages[this.Page] = new Page();
            this.State = 2;
            this.x = this.LeftMargin;
            this.y = this.TopMargin;
            this.FontFamily = "";
            // Check page size and orientation
            if (orientation == PageOrientation.Default)
            {
                orientation = this.DefOrientation;
            }
            if (size == null)
            {
                size = this.DefPageSize;
            }
            else
            {
                size = this._getpagesize(size);
            }

            if (orientation != this.CurOrientation || size.Width != this.CurPageSize.Width || size.Heigth != this.CurPageSize.Heigth)
            {
                // New size or orientation
                if (orientation == PageOrientation.Portrait)
                {
                    this.w = size.Width;
                    this.h = size.Heigth;
                }
                else
                {
                    this.w = size.Heigth;
                    this.h = size.Width;
                }

                this.wPt = PHP.TypeSupport.ToDouble(this.w) * this.k;
                this.hPt = PHP.TypeSupport.ToDouble(this.h) * this.k;
                this.PageBreakTrigger = PHP.TypeSupport.ToDouble(this.h) - this.PageBreakMargin;
                this.CurOrientation = orientation;
                this.CurPageSize = size;
            }
            if (orientation != this.DefOrientation || size.Width != this.DefPageSize.Width || size.Heigth != this.DefPageSize.Heigth)
            {
                this.PageSizes[this.Page] = new Dimensions() { Width = this.wPt, Heigth = this.hPt };
            }
        }

        internal virtual void _endpage()
        {
            this.State = 1;
        }

        internal virtual FontDefinition _loadfont(string font)
        {
            // Load a font definition file from the font directory
            FontDefinition fontData;
            FontBuilder.Fonts.TryGetValue(font, out fontData);
            if (fontData == null || string.IsNullOrEmpty(fontData.name))
            {
                this.Error("Could not include font definition file");
            }
            return fontData;
        }

        internal virtual object _escape(string s)
        {
            s =s
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("\r", "\\r");
            
            /*
            s = PHP.StringSupport.StringReplace(s, "\\", "\\\\");
            s = PHP.StringSupport.StringReplace(s, "(", "\\(");
            s = PHP.StringSupport.StringReplace(s, ")", "\\)");
            s = PHP.StringSupport.StringReplace(s, "\r", "\\r");
            */
            return s;
        }

        internal virtual string _textstring(string s)
        {
            // Format a text string
            return "(" + PHP.TypeSupport.ToString(this._escape(s)) + ")";
        }

        internal virtual string _dounderline(double x, double y, string txt)
        {
            // Underline text
            int up;
            int ut;
            double w;
            up = this.CurrentFont.up;
            ut = this.CurrentFont.ut;
            w = this.GetStringWidth(txt) + this.ws * PHP.StringSupport.SubstringCount(txt, " ");
            return sprintf("%.2F %.2F %.2F %.2F re f", x * this.k, (PHP.TypeSupport.ToDouble(this.h) - (y - up / 1000 * this.FontSize)* this.k) , w * this.k, (-ut) / 1000 * this.FontSizePt);
        }

        internal virtual ImageInfo _parsejpg(string file)
        {
            // Extract info from a JPEG file
            string colspace;
            int bpc;
            List<byte[]> data;

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, file);
            BitmapImage bi=new BitmapImage(new System.Uri(path));

            /*           
                        if (!PHP.TypeSupport.ToBoolean(a))
                        {
                            this.Error("Missing or incorrect image file: " + file);
                        }

                        if (PHP.TypeSupport.ToInt32(a[2]) != 2)
                        {
                            this.Error("Not a JPEG file: " + file);
                        }
                        */
            int channels = 3;
     
            colspace ="DeviceRGB";
            /*
            if (b.PixelFormat== System.Drawing.Imaging.PixelFormat.)
            {
                colspace = "DeviceRGB";
            }
            else if (channels == 4)
                {
                    colspace = "DeviceCMYK";
                }
                else
                {
                    colspace = "DeviceGray";
            }
             * */
            bpc = bi.Format.BitsPerPixel;// (a["bits"] != null) ? PHP.TypeSupport.ToInt32(a["bits"]) : 8;
            data = new List<byte[]> { PHP.FileSystemSupport.ReadContentBytes(file)};
            return new ImageInfo()
            {
                w = bi.PixelWidth,
                h = bi.PixelHeight,
                cs = colspace,
                bpc = bpc,
                f = "DCTDecode",
                data = data
            };
            //return new PHP.OrderedMap(new object[] { "w", b.Width }, new object[] { "h", a[1] }, new object[] { "cs", colspace },
            //    new object[] { "bpc", bpc }, new object[] { "f", "DCTDecode" }, new object[] { "data", data });
        }

        internal virtual ImageInfo _parsepng(string file)
        {
            // Extract info from a PNG file
            System.IO.FileStream f;
            ImageInfo info;
            f = PHP.FileSystemSupport.FileOpen(file, "rb");
            if (!PHP.TypeSupport.ToBoolean(f))
            {
                this.Error("Can\'t open image file: " + file);
            }

            EndianBitConverter converter = new BigEndianBitConverter();

            EndianBinaryReader reader = new EndianBinaryReader(converter ,   f, System.Text.Encoding.ASCII);

            info = this._parsepngstream(f, reader, file);
            reader.Close();
            return info;
        }

        internal virtual ImageInfo _parsepngstream(FileStream f, EndianBinaryReader reader, string file)
        {
            int w;
            int height;
            int bpc;
            int ct;
            string colspace;
            string dp;
            byte[] pal;
            int[] trns;
            byte[] data = new byte[] { };
            int n;
            string type;
            string t;
            int pos;
            ImageInfo info = new ImageInfo();
            StringBuilder color;
            StringBuilder alpha;
            int len;
            int i;
            string line;
            var signature =this._readstream(reader, 8);
            if (!signature.Contains("PNG"))
            {
                this.Error("Not a PNG file: " + file);
            }
            
            /*
            if ( signature != System.Convert.ToString((char)137) + "PNG" + System.Convert.ToString((char)13) + System.Convert.ToString((char)10) + System.Convert.ToString((char)26) + System.Convert.ToString((char)10))
            {
                this.Error("Not a PNG file: " + file);
            }
            */

            // Read header chunk
            this._readstream(reader, 4);
            if (this._readstream(reader, 4) != "IHDR")
            {
                this.Error("Incorrect PNG file: " + file);
            }
            w = reader.ReadInt32();
            height = reader.ReadInt32();
            bpc = (int)this._readstream(reader, 1)[0];
            if (bpc > 8)
            {
                this.Error("16-bit depth not supported: " + file);
            }
            ct = (int)this._readstream(reader, 1)[0];

            colspace = "DeviceRGB";

            if (ct == 0 || ct == 4)
            {
                colspace = "DeviceGray";
            }
            else if (ct == 2 || ct == 6)
            {
                colspace = "DeviceRGB";
            }
            else if (ct == 3)
            {
                colspace = "Indexed";
            }
            else
            {
                this.Error("Unknown color type: " + file);
            }
            if ((int)this._readstream(reader, 1)[0] != 0)
            {
                this.Error("Unknown compression method: " + file);
            }
            if ((int)this._readstream(reader, 1)[0] != 0)
            {
                this.Error("Unknown filter method: " + file);
            }
            if ((int)this._readstream(reader, 1)[0] != 0)
            {
                this.Error("Interlacing not supported: " + file);
            }
            this._readstream(reader, 4);
            dp = "/Predictor 15 /Colors " + ((colspace == "DeviceRGB") ? 3 : 1).ToString() + " /BitsPerComponent " + bpc.ToString() + " /Columns " + w.ToString();

            // Scan chunks looking for palette, transparency and image data
            pal = new byte[] {};
            trns = new int[] { };
            data = new byte[] { };
            do
            {
                n = reader.ReadInt32();
                type = this._readstream(reader, 4);
                if (type == "PLTE")
                {
                    // Read palette
                    pal = this._readStreamBytes(reader, n);
                    this._readstream(reader, 4);
                }
                else if (type == "tRNS")
                {
                    // Read transparency info
                    t = this._readstream(reader, n);
                    if (ct == 0)
                    {
                        trns = new int[] { Convert.ToInt32(t[1]) }; // new PHP.OrderedMap((int)t.Substring(1, 1)[0]);
                    }
                    else if (ct == 2)
                    {
                        //trns = new PHP.OrderedMap((int)t.Substring(1, 1)[0], (int)t.Substring(3, 1)[0], (int)t.Substring(5, 1)[0]);
                        trns = new int[] { Convert.ToInt32(t[1]), Convert.ToInt32(t[3]), Convert.ToInt32(t[5]) };
                    }
                    else
                    {
                        pos = t.IndexOf(System.Convert.ToString((char)0));
                        if (pos > 0)
                        {
                            trns = new int[] { pos };
                        }
                    }
                    this._readstream(reader, 4);
                }
                else if (type == "IDAT")
                {
                    // Read image data block
                    data = _readStreamBytes(reader, n);
                    this._readstream(reader, 4);
                }

                else if (type == "IEND")
                {
                    break;
                }
                else
                {
                    this._readstream(reader, n + 4);
                }
            }
            while (System.Convert.ToBoolean(n));

            if (colspace == "Indexed" && PHP.VariableSupport.Empty(pal))
            {
                this.Error("Missing palette in " + file);
            }
            info =
                new ImageInfo()
                {
                    w = w,
                    h = height,
                    cs = colspace,
                    bpc = bpc,
                    f = "FlateDecode",
                    dp = dp,
                    pal = pal,
                    trns = trns
                };
            //new PHP.OrderedMap(new object[] { "w", w }, new object[] { "h", h }, new object[] { "cs", colspace }, new object[] { "bpc", bpc }, new object[] { "f", "FlateDecode" }, new object[] { "dp", dp }, new object[] { "pal", pal }, new object[] { "trns", trns });


            if (ct >= 4)
            {
                // Extract alpha channel
                string newData = GzUncompressString(data);
                color = new StringBuilder();
                alpha = new StringBuilder();
                if (ct == 4)
                {
                    // Gray image
                    len = 2 * w;
                    for (i = 0; i < height; i++)
                    {
                        pos = (1 + len) * i;
                        color.Append(newData[pos]);
                        alpha.Append(newData[pos]);
                        line = newData.Substring(pos + 1, len);
                        //color.Append(new System.Text.RegularExpressions.Regex("/(.)./s").Replace(line, "$1"));
                        //alpha.Append(new System.Text.RegularExpressions.Regex("/.(.)/s").Replace(line, "$1"));
                        for (int posLinea = 0; posLinea < line.Length; posLinea += 2)
                        {
                            color.Append(line[posLinea]);
                            alpha.Append(line[posLinea + 1]);
                        }
                    }
                }
                else
                {
                    // RGB image
                    len = 4 * w;
                    for (i = 0; i < height; i++)
                    {
                        pos = (1 + len) * i;
                        color.Append(newData[pos]);
                        alpha.Append(newData[pos]);
                        line = newData.Substring(pos + 1, len);
                        for (int posLinea = 0; posLinea < line.Length; posLinea += 4 )
                        {
                            color.Append(line.Substring(posLinea, 3));
                            alpha.Append(line[posLinea + 3]);
                        }
                        //color.Append(new Regex("/(.{3})./s").Replace(line, new MatchEvaluator(FPdf.CapText)));
                        //alpha.Append(new Regex("/.{3}(.)/s").Replace(line, "$1"));
                    }
                }
                data = GzCompressString(color.ToString());
                info.data.Add(data);
                info.smask = GzCompressString(alpha.ToString());
                if (this.PDFVersion.CompareTo("1.4") < 0)
                {
                    this.PDFVersion = "1.4";
                }
            }
            else
            {
                info.data = new List<byte[]>() { data };
            } return info;
        }


        static string CapText(Match m)
        {
            // Get the matched string.
            string x = m.ToString();
            // If the first char is lower case...
            if (char.IsLower(x[0]))
            {
                // Capitalize it.
                return char.ToUpper(x[0]) + x.Substring(1, x.Length - 1);
            }
            return x;
        }


        internal virtual byte[] _readStreamBytes(EndianBinaryReader br, int n)
        {
            byte[] result = br.ReadBytes(n);
            return result;
        }

        internal virtual string _readstream(EndianBinaryReader br, int n)
        {
            // Read n bytes from stream
            string res;
            string s;
            res = "";

            while (n > 0 && !(br.BaseStream.Position >= br.BaseStream.Length))
            {
                s = PHP.FileSystemSupport.Read(br, n);
                if (s == null)
                {
                    this.Error("Error while reading stream");
                }
                n -= s.Length;
                res += s;
            }
            if (n > 0)
            {
                this.Error("Unexpected end of stream");
            }
            return res;
        }

        internal virtual Int32 _readint(System.IO.FileStream f, BinaryReader br)
        {
            // Read a 4-byte integer from stream
            //PHP.OrderedMap a;
            //CONVERSION_ISSUE: Method 'unpack' was not converted. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1000.htm 
            //a = unpack("Ni", this._readstream(f, 4));
            //return PHP.TypeSupport.ToInt32(a["i"]);
            Int32 a = br.ReadInt32();
            return a;
        }

        internal virtual ImageInfo _parsegif(string file)
        {
            throw new NotImplementedException();
            /*
            // Extract info from a GIF file (via PNG conversion)
            int im;
            System.IO.FileStream f;
            string data;
            PHP.OrderedMap info;
            string tmp;
            if (!(this.GetType().GetMethod("imagepng") != null))
            {
                this.Error("GD extension is required for GIF support");
            }
            if (!(this.GetType().GetMethod("imagecreatefromgif") != null))
            {
                this.Error("GD has no GIF read support");
            }
            im = imagecreatefromgif(file);
            if (!System.Convert.ToBoolean(im))
            {
                this.Error("Missing or incorrect image file: " + file);
            }
            imageinterlace(im, 0);
            try
            {
                //CONVERSION_WARNING: Method 'fopen' was converted to 'PHP.FileSystemSupport.FileOpen' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/fopen.htm 
                f = PHP.FileSystemSupport.FileOpen("php://temp", "rb+");
            }
            catch (System.Exception)
            {
            }
            if (PHP.TypeSupport.ToBoolean(f))
            {
                // Perform conversion in memory
                ob_start();
                imagepng(im);
                data = ob_get_clean() ? "1" : "";
                imagedestroy(im);
                //CONVERSION_WARNING: Method 'fwrite' was converted to 'PHPFileSystemSupport.Write' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/fwrite.htm 
                PHP.FileSystemSupport.Write(f, data, -1);
                //CONVERSION_WARNING: Method 'rewind' was converted to 'PHPFileSystemSupport.Rewind' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/rewind.htm 
                PHP.FileSystemSupport.Rewind(f);
                info = this._parsepngstream(f, file);
                //CONVERSION_WARNING: Method 'fclose' was converted to 'PHP.FileSystemSupport.Close' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/fclose.htm 
                PHP.FileSystemSupport.Close(f);
            }
            else
            {
                // Use temporary file
                //CONVERSION_WARNING: Method 'tempnam' was converted to 'System.IO.Path.GetTempFileName' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/tempnam.htm 
                tmp = System.IO.Path.GetTempFileName();
                if (!PHP.TypeSupport.ToBoolean(tmp))
                {
                    this.Error("Unable to create a temporary file");
                }
                if (!System.Convert.ToBoolean(imagepng(im, tmp)))
                {
                    this.Error("Error while saving to temporary file");
                }
                imagedestroy(im);
                info = this._parsepng(tmp);
                //CONVERSION_WARNING: Method 'unlink' was converted to 'System.IO.File.Delete' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/unlink.htm 
                System.IO.File.Delete(tmp);
            }
            return info;

             */
        }

        internal virtual void _newobj()
        {
            // Begin a new object
            this.n++;
            this.Offsets[this.n] = this.Buffer.Length;
            this._out(this.n.ToString() + " 0 obj");
        }

        internal virtual void _putstream(string s)
        {
            this._out("stream");
            this._out(s);
            this._out("endstream");
        }

        internal virtual void _putstream(byte[] bytes)
        {
            this._out("stream");
            this._out(bytes);
            this._out("endstream");
        }

        internal virtual void _putstream(List<byte[]> bytes)
        {
            this._out("stream");
            this._out(bytes);
            this._out("endstream");
        }

        internal virtual void _out(object s)
        {
            // Add a line to the document
            if (this.State == 2)
            {
                //TODO: APPENDLN ?
                this.Pages[this.Page]
                    .Append(PHP.TypeSupport.ToString(s));
                this.Pages[this.Page]
                    .Append("\n");
            }
            else
            {
                if (s is List<byte[]>)
                {
                    foreach (var v in (s as List<byte[]>))
                    {
                        this.Buffer += PrivateEnconding.GetString((byte[])v);
                    }
                    this.Buffer += "\n";
                }
                else if (s is byte[])
                {
                    this.Buffer += PrivateEnconding.GetString((byte[])s);
                    this.Buffer += "\n";
                }
                else
                {
                    this.Buffer += PHP.TypeSupport.ToString(s) + "\n";
                }
            }
        }

        internal virtual void _putpages()
        {
            int nb;
            int n;
            double wPt;
            double hPt;
            string filter;
            string annots;
            string rect;
            double h;
            byte[] p;
            string kids;
            int i;
            nb = this.Page;
            if (!PHP.VariableSupport.Empty(this.AliasNbPages_Renamed))
            {
                // Replace number of pages
                for (n = 1; n <= nb; n++)
                {
                    //string page = Convert.ToString(this.pages[n]);
                    this.Pages[n].Replace(this.AliasNbPages_Renamed, nb.ToString());
                }
            }
            if (this.DefOrientation == PageOrientation.Portrait)
            {
                wPt = PHP.TypeSupport.ToDouble(this.DefPageSize.Width) * this.k;
                hPt = PHP.TypeSupport.ToDouble(this.DefPageSize.Heigth) * this.k;
            }
            else
            {
                wPt = PHP.TypeSupport.ToDouble(this.DefPageSize.Heigth) * this.k;
                hPt = PHP.TypeSupport.ToDouble(this.DefPageSize.Width) * this.k;
            }
            filter = (this.Compress) ? "/Filter /FlateDecode " : "";
            for (n = 1; n <= nb; n++)
            {
                // Page
                this._newobj();
                this._out("<</Type /Page");
                this._out("/Parent 1 0 R");
                if (this.PageSizes.ContainsKey(n))
                {
                    this._out(sprintf("/MediaBox [0 0 %.2F %.2F]", this.PageSizes[n].Width, this.PageSizes[n].Heigth));
                    //this._out(sprintf("/MediaBox [0 0 %.2F %.2F]", this.PageSizes[n].Widht, this.PageSizes[n].Height));
                }
                this._out("/Resources 2 0 R");
                if (this.Pages[n].PageLinks.Count > 0)
                {
                    // Links
                    annots = "/Annots [";
                    foreach (var pl in this.Pages[n].PageLinks)
                    {
                        rect = sprintf("%.2F %.2F %.2F %.2F", pl.P0, pl.P1, pl.P0 + pl.P2, pl.P1 - pl.P3);
                        annots += "<</Type /Annot /Subtype /Link /Rect [" + rect + "] /Border [0 0 0] ";


                        if (pl.Link is LinkDataInternal)
                        {
                            var link = this.links[(pl.Link as LinkDataInternal).InternalLink];
                            int l0 = link.PageIndex;
                            h = (this.PageSizes.ContainsKey(l0)) ? PHP.TypeSupport.ToDouble(this.PageSizes[l0].Heigth) : hPt;
                            annots += sprintf("/Dest [%d 0 R /XYZ 0 %.2F null]>>", 1 + 2 * link.PageIndex, h - link.Y * this.k);

                        }
                        else if (pl.Link is LinkDataUri)
                        {
                            annots += "/A <</S /URI /URI " + (pl.Link as LinkDataUri).Uri + ">>>>";
                        }
                        else
                            throw new NotImplementedException();
                    }
                    this._out(annots + "]");
                }
                if (this.PDFVersion.CompareTo("1.3") > 0)
                {
                    this._out("/Group <</Type /Group /S /Transparency /CS /DeviceRGB>>");
                }
                this._out("/Contents " + (this.n + 1).ToString() + " 0 R>>");
                this._out("endobj");
                // Page content
                if (this.Compress)
                {
                    p = GzCompressString(this.Pages[n].ToString());
                    this._newobj();
                    this._out("<<" + filter + "/Length " + p.Length.ToString() + ">>");
                    this._putstream(p);
                    this._out("endobj");
                }
                else
                {
                    string p1 = this.Pages[n].ToString();
                    this._newobj();
                    this._out("<<" + filter + "/Length " + p1.Length.ToString() + ">>");
                    this._putstream(p1);
                    this._out("endobj");
                }
            }
            // Pages root
            this.Offsets[1] = this.Buffer.Length;
            this._out("1 0 obj");
            this._out("<</Type /Pages");
            kids = "/Kids [";
            for (i = 0; i < nb; i++)
                kids += (3 + 2 * i).ToString() + " 0 R ";
            this._out(kids + "]");
            this._out("/Count " + nb.ToString());
            this._out(sprintf("/MediaBox [0 0 %.2F %.2F]", wPt, hPt));
            this._out(">>");
            this._out("endobj");
        }

        internal virtual void _putfonts()
        {
            int nf;
            bool compressed;
            FontTypeEnum type;
            string name;
            PHP.OrderedMap cw;
            string s;
            int i;
            string mtd;
            string font;
            nf = this.n;
            foreach (object diff in this.diffs.Values)
            {
                // Encodings
                this._newobj();
                this._out("<</Type /Encoding /BaseEncoding /WinAnsiEncoding /Differences [" + PHP.TypeSupport.ToString(diff) + "]>>");
                this._out("endobj");
            }

            foreach (string file in this.FontFiles.Keys)
            {
                FontDefinition info = this.Fonts[file];
                // Font file embedding
                this._newobj();
                info.n = this.n;
                //file_get_contents' returns a string 
                font = PHP.FileSystemSupport.ReadContents(this.fontpath + file);
                if (string.IsNullOrWhiteSpace(font))
                {
                    this.Error("Font file not found: " + file);
                }
                string extension = Path.GetExtension(file);
                compressed = (extension == ".z");
                if (!compressed && info.length2 > 0)
                {
                    font = PHP.TypeSupport.ToString(font).Substring(6, info.length1)
                        + PHP.TypeSupport.ToString(font).Substring(6 + info.length1 + 6, info.length2);
                }
                this._out("<</Length " + font.Length.ToString());
                if (compressed)
                {
                    this._out("/Filter /FlateDecode");
                }
                this._out("/Length1 " + PHP.TypeSupport.ToString(info.length1));
                if (info.length2 != 0)
                {
                    this._out("/Length2 " + PHP.TypeSupport.ToString(info.length2) + " /Length3 0");
                }
                this._out(">>");
                this._putstream(font);
                this._out("endobj");
            }

            foreach (string k in this.Fonts.Keys)
            {
                FontDefinition font1 = this.Fonts[k];
                // Font objects
                font1.n = this.n + 1;
                type = font1.type;
                name = font1.name;
                if (type == FontTypeEnum.Core)
                {
                    // Core font
                    this._newobj();
                    this._out("<</Type /Font");
                    this._out("/BaseFont /" + PHP.TypeSupport.ToString(name));
                    this._out("/Subtype /Type1");
                    if (PHP.TypeSupport.ToString(name) != "Symbol" && PHP.TypeSupport.ToString(name) != "ZapfDingbats")
                    {
                        this._out("/Encoding /WinAnsiEncoding");
                    }
                    this._out(">>");
                    this._out("endobj");
                }
                else if (type == FontTypeEnum.Type1 || type == FontTypeEnum.TrueType)
                {
                    // Additional Type1 or TrueType/OpenType font
                    this._newobj();
                    this._out("<</Type /Font");
                    this._out("/BaseFont /" + PHP.TypeSupport.ToString(name));
                    this._out("/Subtype /" + PHP.TypeSupport.ToString(type));
                    this._out("/FirstChar 32 /LastChar 255");
                    this._out("/Widths " + (this.n + 1).ToString() + " 0 R");
                    this._out("/FontDescriptor " + (this.n + 2).ToString() + " 0 R");
                    if (font1.diffn.HasValue)
                    {
                        this._out("/Encoding " + (nf + font1.diffn).ToString() + " 0 R");
                    }
                    else
                    {
                        this._out("/Encoding /WinAnsiEncoding");
                    }
                    this._out(">>");
                    this._out("endobj");
                    // Widths
                    this._newobj();
                    cw = PHP.TypeSupport.ToArray(font1.cw);
                    s = "[";
                    for (i = 32; i <= 255; i++)
                        s += PHP.TypeSupport.ToString(cw[System.Convert.ToString((char)i)]) + " ";
                    this._out(s + "]");
                    this._out("endobj");
                    // Descriptor
                    this._newobj();
                    s = "<</Type /FontDescriptor /FontName /" + PHP.TypeSupport.ToString(name);
                    foreach (string k1 in font1.desc.Keys)
                    {
                        object v = font1.desc[k1];
                        s += " /" + k1 + " " + PHP.TypeSupport.ToString(v);
                    }

                    if (!string.IsNullOrEmpty(font1.file))
                    {
                        s += " /FontFile" + (type == FontTypeEnum.Type1 ? "" : "2") + " " + PHP.TypeSupport.ToString(this.Fonts[font1.file].n) + " 0 R";
                    }
                    this._out(s + ">>");
                    this._out("endobj");
                }
                else
                {
                    this.Error("Unsupported font type: " + PHP.TypeSupport.ToString(type));
                }
            }
        }

        internal virtual void _putimages()
        {
            foreach (var file in this.images)
            {
                this._putimage(file.Value);
                file.Value.data = null; //unset, probably not needed
                file.Value.smask = null; //unset, probably not needed
            }
        }

        internal virtual void _putimage(ImageInfo info)
        {
            string trns;
            string dp;
            ImageInfo smask;
            string filter;
            byte[] pal;
            this._newobj();
            info.n = this.n;
            this._out("<</Type /XObject");
            this._out("/Subtype /Image");
            this._out("/Width " + info.w.ToString());
            this._out("/Height " + info.h.ToString());
            if (info.cs == "Indexed")
            {
                this._out("/ColorSpace [/Indexed /DeviceRGB " + (info.pal.Length / 3 - 1).ToString() + " " + (this.n + 1).ToString() + " 0 R]");
            }
            else
            {
                this._out("/ColorSpace /" + PHP.TypeSupport.ToString(info.cs));
                if (PHP.TypeSupport.ToString(info.cs) == "DeviceCMYK")
                {
                    this._out("/Decode [1 0 1 0 1 0 1 0]");
                }
            }
            this._out("/BitsPerComponent " + PHP.TypeSupport.ToString(info.bpc));
            if (info.f != null)
            {
                this._out("/Filter /" + PHP.TypeSupport.ToString(info.f));
            }
            if (info.dp != null)
            {
                this._out("/DecodeParms <<" + PHP.TypeSupport.ToString(info.dp) + ">>");
            }
            if (info.trns != null && info.trns.Count() > 0)
            {
                trns = "";
                foreach (var trn in info.trns)
                {
                    trns += trn + " " + trn + " ";
                }
                this._out("/Mask [" + trns + "]");
            }
            if (info.smask != null)
            {
                this._out("/SMask " + (this.n + 1).ToString() + " 0 R");
            }

            int largo = info.data.Select(x => x.Length).Sum();

            this._out("/Length " + largo.ToString() + ">>");
            this._putstream(info.data);
            this._out("endobj");
            // Soft mask
            //CONVERSION_WARNING: Method 'isset' was converted to '!=' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/isset.htm 
            if (info.smask != null)
            {
                dp = "/Predictor 15 /Colors 1 /BitsPerComponent 8 /Columns " + PHP.TypeSupport.ToString(info.w);
                smask = new ImageInfo()
                {
                    w = info.w,
                    h = info.h,
                    cs = "DeviceGray",
                    bpc = 8,
                    f = info.f,
                    dp = dp,
                    data = new List<byte[]> { info.smask }
                };
                /*
                smask = new PHP.OrderedMap(
                        new object[] { "w", info.w }, 
                        new object[] { "h", info.h },
                        new object[] { "cs", "DeviceGray" }, 
                        new object[] { "bpc", 8 },
                        new object[] { "f", info.f }, 
                        new object[] { "dp", dp },
                        new object[] { "data", info.smask });
                */
                this._putimage(smask);
            }
            // Palette
            if (PHP.TypeSupport.ToString(info.cs) == "Indexed")
            {
                filter = (this.Compress) ? "/Filter /FlateDecode " : "";
                if (this.Compress)
                {
                    pal = gzcompress(info.pal);
                }
                else
                {
                    pal = info.pal;
                }
                this._newobj();
                this._out("<<" + filter + "/Length " + pal.Length.ToString() + ">>");
                this._putstream(pal);
                this._out("endobj");
            }
        }

        internal virtual void _putxobjectdict()
        {
            foreach (ImageInfo image in this.images.Values)
            {
                this._out("/I" + PHP.TypeSupport.ToString(image.i) + " " + PHP.TypeSupport.ToString(image.n) + " 0 R");
            }
        }

        internal virtual void _putresourcedict()
        {
            this._out("/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]");
            this._out("/Font <<");
            foreach (FontDefinition font in this.Fonts.Values)
            {
                this._out("/F" + PHP.TypeSupport.ToString(font.i) + " " + PHP.TypeSupport.ToString(font.n) + " 0 R");
            }
            this._out(">>");
            this._out("/XObject <<");
            this._putxobjectdict();
            this._out(">>");
        }

        internal virtual void _putresources()
        {
            this._putfonts();
            this._putimages();
            // Resource dictionary
            this.Offsets[2] = this.Buffer.Length;
            this._out("2 0 obj");
            this._out("<<");
            this._putresourcedict();
            this._out(">>");
            this._out("endobj");
        }

        //CONVERSION_ISSUE: Operator '@' was not converted. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1000.htm 
        internal virtual void _putinfo()
        {
            this._out("/Producer " + this._textstring("FPDF " + FPDF_VERSION));
            if (!PHP.VariableSupport.Empty(this.Title))
            {
                this._out("/Title " + this._textstring(this.Title));
            }
            if (!PHP.VariableSupport.Empty(this.Subject))
            {
                this._out("/Subject " + this._textstring(this.Subject));
            }
            if (!PHP.VariableSupport.Empty(this.Author))
            {
                this._out("/Author " + this._textstring(this.Author));
            }
            if (!PHP.VariableSupport.Empty(this.Keywords))
            {
                this._out("/Keywords " + this._textstring(this.Keywords));
            }
            if (!PHP.VariableSupport.Empty(this.Creator))
            {
                this._out("/Creator " + this._textstring(this.Creator));
            }
            //CONVERSION_WARNING: Method 'date' was converted to 'System.DateTime.ToString' which has a different behavior. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/date.htm 
            //CONVERSION_ISSUE: Operator '@' was not converted. Copy this link in your browser for more info: ms-its:C:\Program Files\Microsoft Corporation\PHP to ASP.NET Migration Assistant\PHPToAspNet.chm::/1000.htm 
            this._out("/CreationDate " + this._textstring("D:" + @System.DateTime.Now.ToString("YmdHis")));
        }

        internal virtual void _putcatalog()
        {
            this._out("/Type /Catalog");
            this._out("/Pages 1 0 R");
            if (this.ZoomMode == "fullpage")
            {
                this._out("/OpenAction [3 0 R /Fit]");
            }
            else if (this.ZoomMode == "fullwidth")
            {
                this._out("/OpenAction [3 0 R /FitH null]");
            }
            else if (this.ZoomMode == "real")
            {
                this._out("/OpenAction [3 0 R /XYZ null null 1]");
            }
            else if (!(string.IsNullOrEmpty(this.ZoomMode)))
            {
                this._out("/OpenAction [3 0 R /XYZ null null " + sprintf("%.2F", PHP.TypeSupport.ToDouble(this.ZoomMode) / 100) + "]");
            }
            if (this.LayoutMode == "single")
            {
                this._out("/PageLayout /SinglePage");
            }
            else if (this.LayoutMode == "continuous")
            {
                this._out("/PageLayout /OneColumn");
            }
            else if (this.LayoutMode == "two")
            {
                this._out("/PageLayout /TwoColumnLeft");
            }
        }

        internal virtual void _putheader()
        {
            this._out("%PDF-" + this.PDFVersion);
        }

        internal virtual void _puttrailer()
        {
            this._out("/Size " + (this.n + 1).ToString());
            this._out("/Root " + this.n.ToString() + " 0 R");
            this._out("/Info " + (this.n - 1).ToString() + " 0 R");
        }

        internal virtual void _enddoc()
        {
            object o;
            int i;
            this._putheader();
            this._putpages();
            this._putresources();
            // Info
            this._newobj();
            this._out("<<");
            this._putinfo();
            this._out(">>");
            this._out("endobj");
            // Catalog
            this._newobj();
            this._out("<<");
            this._putcatalog();
            this._out(">>");
            this._out("endobj");
            // Cross-ref
            o = this.Buffer.Length;
            this._out("xref");
            this._out("0 " + (this.n + 1).ToString());
            this._out("0000000000 65535 f ");
            for (i = 1; i <= this.n; i++)
            {
                this._out(sprintf("%010d 00000 n ", this.Offsets[i]));
                /*
                 * Warning: string.format has a different behaviour for negative numbers
                if (this.offsets[i] < 0)
                {
                    this._out(string.Format(CultureInfo.InvariantCulture, "{0:D9} 00000 n ", this.offsets[i]));
                }
                else
                {
                    this._out(string.Format(CultureInfo.InvariantCulture, "{0:D10} 00000 n ", this.offsets[i]));
                }*/
            }
            // Trailer
            this._out("trailer");
            this._out("<<");
            this._puttrailer();
            this._out(">>");
            this._out("startxref");
            this._out(o);
            this._out("%%EOF");
            this.State = 3;
        }

         public static string sprintf(string Format, params object[] Parameters)
        {
            string result = PDF.SprintfTools.sprintf(Format, Parameters);
            return result;
        }

        public byte[] gzcompress(byte[] value)
        {
            MemoryStream outstream = new MemoryStream();
            Ionic.Zlib.ZlibStream g = new Ionic.Zlib.ZlibStream(outstream, Ionic.Zlib.CompressionMode.Compress);
            g.Write(value, 0, value.Length);
            g.Close();
            var result = outstream.ToArray();
            return result;
        }
        
        public byte[] gzuncompress(byte[] value)
        {
            MemoryStream instream = new MemoryStream(value, false);
            Ionic.Zlib.ZlibStream g = new Ionic.Zlib.ZlibStream(instream, Ionic.Zlib.CompressionMode.Decompress);
            BinaryReader reader = new BinaryReader(g);
            byte[] bytes = reader.ReadBytes(Int16.MaxValue*100);
            g.Close();
            return bytes;
        }

        public string GzUncompressString(byte[] value)
        {
            byte[] uncompressedArray = gzuncompress(value);
            string result = PrivateEnconding.GetString(uncompressedArray);
            return result;
        }

        public byte[] GzCompressString(string value)
        {
            byte[] bytes = PrivateEnconding.GetBytes(value);
            byte[] result = gzcompress(bytes);
            return result;
        }
    }
}