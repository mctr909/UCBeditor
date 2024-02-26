using System;
using System.Drawing;

public interface IDrawer : IDisposable {
	Color DrawColor { set; }
	Color FillColor { set; }
	double FontSize { get; set; }

	SizeF GetTextSize(string s);

	void ClearTranslate();
	void SetTranslate(double x, double y);

	void DrawText(string s, double x, double y, bool centering = false);
	void DrawText(string s, PointF p, bool centering = false);

	void DrawText(string s, double x, double y, double rotateAngle, bool centering = false);
	void DrawText(string s, PointF p, double rotateAngle, bool centering = false);

	void FillLine(double ax, double ay, double bx, double by, double weight);
	void FillLine(PointF a, PointF b, double weight);

	void FillCircle(double x, double y, double diameter);
	void FillCircle(PointF p, double diameter);

	void FillPie(double x, double y, double diameter, double start, double sweep);
	void FillPie(PointF p, double diameter, double start, double sweep);

	void FillArc(double x, double y, double diameter, double start, double sweep, double weight);
	void FillArc(PointF p, double diameter, double start, double sweep, double weight);

	void FillPolygon(params PointF[] poly);
}
