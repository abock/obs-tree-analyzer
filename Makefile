ASSEMBLY = obs-tree-analyzer.exe
SOURCES = $(shell find ObsTreeAnalyzer)

$(ASSEMBLY): $(SOURCES)
	mdtool build ObsTreeAnalyzer/ObsTreeAnalyzer.sln

run: $(ASSEMBLY)
	./analyze trees/Moblin\:Factory
	gnome-open trees/Moblin\:Factory/report.html

clean:
	rm -f $(ASSEMBLY){,.mdb}
