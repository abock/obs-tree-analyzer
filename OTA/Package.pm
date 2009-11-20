package Package;
use OTA::Node;
our @ISA = qw(Node);

use strict;
use warnings;

sub new {
	my ($class, $name) = @_;
	my $self = $class->SUPER::new;
	bless $self, $class;
	return $self;
}

sub Load {
	my ($self, $dirname) = @_;
	my $xp = XML::XPath->new (filename => "$dirname/.osc/_files");
	$self->{_name} = $xp->findvalue ('/directory/@name');
	foreach ($xp->find ('/directory/entry/@name')->get_nodelist) {
		my $name = $_->getNodeValue;
		my $entry = new Node;
		$entry->{_name} = $name;
		push @{$self->{_children}}, $entry;
	}
}

1;
