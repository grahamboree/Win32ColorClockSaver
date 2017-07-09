using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ColorClockSaver {
	public partial class MainForm : Form {
		#region Preview API's
		[DllImport("user32.dll")]
		static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);
		#endregion

		bool IsPreviewMode;

		#region Constructors
		public MainForm() {
			InitializeComponent();
		}

		/// This constructor is passed the bounds this form is to show in
		/// It is used when in normal mode
		public MainForm(Rectangle Bounds) {
			InitializeComponent();
			this.Bounds = Bounds;
			Cursor.Hide();
		}

		/// This constructor is the handle to the select screensaver dialog preview window
		/// It is used when in preview mode (/p)
		public MainForm(IntPtr PreviewHandle) {
			InitializeComponent();

			// set the preview window as the parent of this window
			SetParent(Handle, PreviewHandle);

			// make this a child window, so when the select screensaver dialog closes, this will also close
			SetWindowLong(Handle, -16, new IntPtr(GetWindowLong(Handle, -16) | 0x40000000));

			// set our window's size to the size of our window's new parent
			Rectangle ParentRect;
			GetClientRect(PreviewHandle, out ParentRect);
			Size = ParentRect.Size;

			// set our location at (0, 0)
			Location = new Point(0, 0);

			IsPreviewMode = true;
		}
		#endregion

		#region GUI
		void MainForm_Shown(object sender, EventArgs e) {
			DoubleBuffered = true;

			if (!IsPreviewMode) {
				Refresh();
				var timer = new Timer {
					Interval = 1000 / 60 // 60 FPS
				};
				timer.Tick += (o, args) => { Refresh(); };
				timer.Start();
			}
		}
		#endregion

		#region User Input
		void MainForm_KeyDown(object sender, KeyEventArgs e) {
			// disable exit functions for preview
			if (!IsPreviewMode) {
				Application.Exit();
			}
		}

		void MainForm_Click(object sender, EventArgs e) {
			// disable exit functions for preview
			if (!IsPreviewMode) {
				Application.Exit();
			}
		}

		/// start off OriginalLoction with an X and Y of int.MaxValue, because
		/// it is impossible for the cursor to be at that position. That way, we
		/// know if this variable has been set yet.
		Point OriginalLocation = new Point(int.MaxValue, int.MaxValue);

		void MainForm_MouseMove(object sender, MouseEventArgs e) {
			// disable exit functions for preview
			if (IsPreviewMode) {
				return;
			}

			// see if originallocation has been set
			if (OriginalLocation.X == int.MaxValue & OriginalLocation.Y == int.MaxValue) {
				OriginalLocation = e.Location;
			}
			// see if the mouse has moved more than 20 pixels in any direction. If it has, close the application.
			if (Math.Abs(e.X - OriginalLocation.X) > 20 | Math.Abs(e.Y - OriginalLocation.Y) > 20) {
				Application.Exit();
			}
		}
		#endregion

		void MainForm_Paint(object sender, PaintEventArgs e) {
			var graphics = e.Graphics;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			var nowColor = DateTime.Now.ToColor();
			
			// Draw the background.
			using (GraphicsPath gp = new GraphicsPath()) {
				// This is how we do a radial gradient that covers the whole screen.
				gp.AddEllipse(new Rectangle(-Bounds.Width / 2, -Bounds.Height / 2, Bounds.Width * 2, Bounds.Height * 2));
				using (var pgb = new PathGradientBrush(gp)) {
					pgb.CenterPoint = new PointF(Width / 2f, Height / 2f);
					pgb.CenterColor = nowColor;
					pgb.SurroundColors = new[] { nowColor.DarkerVersion() };
					e.Graphics.FillPath(pgb, gp);
				}
			}

			int yMedian = (int)(Height / 2f + Width * 0.05f);
			float timeFontSize = Width * 0.1f;

			// Time text
			graphics.DrawString(
				DateTime.Now.ToString("HH:mm:ss"),
				new Font("mononoki", Width * 0.1f, FontStyle.Regular, GraphicsUnit.Pixel),
				nowColor.IsLight() ? Brushes.Black : Brushes.White,
				new Rectangle(0, 0, Width, yMedian),
				new StringFormat {
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Far
				});

			yMedian -= (int)(0.25f * timeFontSize);

			// Color code text
			graphics.DrawString(
				nowColor.ToHex(),
				new Font("mononoki", Width * 0.0212f, FontStyle.Regular, GraphicsUnit.Pixel),
				nowColor.IsLight() ? Brushes.Black : Brushes.White,
				new Rectangle(0, yMedian, Width, Height / 2),
				new StringFormat {
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Near
				});
		}
	}

	static class Extensions {
		public static Color ToColor(this DateTime time) {
			return Color.FromArgb(
				(int)Math.Floor(255.0 * time.Hour / 23.0),
				(int)Math.Floor(255.0 * time.Minute / 59.0),
				(int)Math.Floor(255.0 * time.Second / 59.0)
			);
		}

		public static string ToHex(this Color color) {
			return string.Join(null, new[] {
				color.R.ToString("X2"),
				color.G.ToString("X2"),
				color.B.ToString("X2")
			});
		}

		public static float Luma(this Color color) {
			return 0.2126f * (color.R / 255f) +
				0.7152f * (color.G / 255f) +
				0.0722f * (color.B / 255f);
		}

		public static bool IsLight(this Color color) {
			return color.Luma() >= 0.6f;
		}

		public static Color DarkerVersion(this Color color) {
			return Color.FromArgb(
				(int)(color.R * 0.6f),
				(int)(color.G * 0.6f),
				(int)(color.B * 0.6f));
		}
	}
}
