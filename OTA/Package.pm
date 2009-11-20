package Package;
use OTA::Node;
our @ISA = qw(Node);

use strict;
use warnings;

sub new {
	my ($class, $name) = @_;
	my $self = $class->SUPER::new;
	$self->{_name} = $name if $name;
	bless $self, $class;
	return $self;
}

1;
