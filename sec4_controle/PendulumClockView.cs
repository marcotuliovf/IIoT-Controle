using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace sec4_controle.Views
{
    public class PendulumClockView : GraphicsView
    {
        private float _angle;
        private readonly float _clockRadius = 100;

        public float Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                Invalidate();
            }
        }

        public PendulumClockView()
        {
            HeightRequest = 250;
            WidthRequest = 250;

            Drawable = new PendulumClockDrawable(this);
        }

        private class PendulumClockDrawable : IDrawable
        {
            private readonly PendulumClockView _view;

            public PendulumClockDrawable(PendulumClockView view)
            {
                _view = view;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var center = new PointF(dirtyRect.Width / 2, dirtyRect.Height / 2);

                // Fundo do relógio
                canvas.FillColor = Colors.White;
                canvas.FillCircle(center, _view._clockRadius);

                // Borda do relógio
                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = 3;
                canvas.DrawCircle(center, _view._clockRadius);

                // Marcações de hora + ângulos
                for (int i = 0; i < 12; i++)
                {
                    double angleRad = i * Math.PI / 6;
                    float sin = (float)Math.Sin(angleRad);
                    float cos = (float)Math.Cos(angleRad);

                    float innerRadius = (i % 3 == 0) ? _view._clockRadius - 20 : _view._clockRadius - 12;

                    var inner = new PointF(
                        center.X + innerRadius * sin,
                        center.Y - innerRadius * cos);

                    var outer = new PointF(
                        center.X + _view._clockRadius * sin,
                        center.Y - _view._clockRadius * cos);

                    canvas.StrokeSize = (i % 3 == 0) ? 2.5f : 1.5f;
                    canvas.DrawLine(inner, outer);

                    // Desenhar o número do ângulo
                    int angleDegree = i * 30;
                    string angleText = $"{angleDegree}°"; // Já coloca o ° ao lado

                    var textPosition = new PointF(
                        center.X + (_view._clockRadius - 30) * sin,
                        center.Y - (_view._clockRadius - 30) * cos);

                    canvas.FontColor = Colors.DarkSlateGray;
                    canvas.FontSize = 10; // Fonte pequena mas mais fácil de ver
                    canvas.DrawString(
                        angleText,
                        textPosition.X - 12,  // Aumenta área para não quebrar o texto
                        textPosition.Y - 7,
                        24, 14, // largura e altura maiores
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center);
                }

                // Ponteiro mais fino e suave
                canvas.StrokeColor = Colors.DarkRed;
                canvas.StrokeSize = 2.5f; // Ponteiro mais refinado ainda

                float angleRadPointer = _view._angle * (float)(Math.PI / 180);
                float sinPointer = (float)Math.Sin(angleRadPointer);
                float cosPointer = (float)Math.Cos(angleRadPointer);

                var endPoint = new PointF(
                    center.X + _view._clockRadius * 0.85f * sinPointer,
                    center.Y - _view._clockRadius * 0.85f * cosPointer);

                canvas.DrawLine(center, endPoint);

                // Centro do relógio
                canvas.FillColor = Colors.Black;
                canvas.FillCircle(center, 5); // Centro ainda mais fino
            }
        }
    }
}
