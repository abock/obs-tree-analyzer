package Node;

use strict;
use warnings;

sub new {
	my ($class) = @_;
	my $self = {
		_name => undef,
		_children => undef
	};
	bless $self, $class;
	return $self;
}

sub Name {
	my ($self) = @_;
	return $self->{_name};
}

sub Children {
	my ($self) = @_;
	return @{$self->{_children}} if $self->{_children};
}

1;
