package Project;
use OTA::Node;
our @ISA = qw(Node);

use strict;
use warnings;
use OTA::Package;
use XML::XPath;
use XML::XPath::XMLParser;

sub new {
	my ($class) = @_;
	my $self = $class->SUPER::new;
	bless $self, $class;
	return $self;
}

sub Load {
	my ($self, $dirname) = @_;
	my $xp = XML::XPath->new (filename => "$dirname/.osc/_packages");
	$self->{_name} = $xp->findvalue ('/project/@name');
	push @{$self->{_children}}, new Package ($_->getNodeValue)
		foreach $xp->find ('/project/package/@name')->get_nodelist;
}

1;
