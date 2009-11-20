package Package;
use OTA::Node;
our @ISA = qw(Node);

use strict;
use warnings;

sub new {
	my ($class, $name) = @_;
	my $self = $class->SUPER::new;
	$self->Name ($name) if $name;
	bless $self, $class;
	return $self;
}

1;
