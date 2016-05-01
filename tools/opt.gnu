#http://www.gnuplotting.org/plotting-functions/
set terminal png size 500,300 enhanced font 'Verdana,10'

# Axes
set border linewidth 1.0
set xlabel 'x'
set xrange [0:30]
set xtics (0, 10, 20, 30)

set ylabel 'y'
set yrange [-.2:1.2]
set ytics 1

set tics scale 0.75

# Line styles
set style line 1 linecolor rgb '#009900' linetype 1 linewidth 1

f(x) = .5 + cos(x/(30/pi)) / 2

# Plot
plot f(x) title 'cos(x)' with lines linestyle 1

