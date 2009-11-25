ASSEMBLIES = text-transform.exe ObsTreeAnalyzer.dll
SOURCES = $(shell find ObsTreeAnalyzer TextTransform)

$(ASSEMBLIES): $(SOURCES)
	mdtool build ObsTreeAnalyzer.sln

run: $(ASSEMBLIES)
	./analyze trees/Moblin\:Factory
	gnome-open "$$PWD/moblin-factory-report.html"

clean:
	rm -f *.exe *.dll *.mdb
